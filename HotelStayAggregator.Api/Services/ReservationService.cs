using HotelStayAggregator.Api.Models;
using HotelStayAggregator.Api.Providers;
using HotelStayAggregator.Api.Repositories;
using HotelStayAggregator.Api.Validation;

namespace HotelStayAggregator.Api.Services;

public interface IReservationService
{
    Task<Reservation> ReserveHotelAsync(ReserveHotelRequest request, CancellationToken cancellationToken = default);
    Task<ReservationLookupResult?> GetReservationAsync(string referenceNumber, CancellationToken cancellationToken = default);
}

public sealed class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IHotelProviderResolver _providerResolver;
    private readonly IReserveHotelRequestValidator _validator;

    public ReservationService(
        IReservationRepository reservationRepository,
        IHotelProviderResolver providerResolver,
        IReserveHotelRequestValidator validator)
    {
        _reservationRepository = reservationRepository;
        _providerResolver = providerResolver;
        _validator = validator;
    }

    public async Task<Reservation> ReserveHotelAsync(ReserveHotelRequest request, CancellationToken cancellationToken = default)
    {
        _validator.Validate(request);

        var provider = _providerResolver.Resolve(request.ProviderName);
        if (provider is null)
        {
            throw new ArgumentException($"Unsupported provider '{request.ProviderName}'.");
        }

        var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
        if (nights <= 0)
        {
            throw new ArgumentException("Check-out date must be after check-in date.");
        }

        var searchRequest = new HotelSearchRequest(request.Destination, request.CheckInDate, request.CheckOutDate, request.RoomType);
        var offers = await provider.SearchHotelsAsync(searchRequest, cancellationToken);
        var hotel = offers.FirstOrDefault(o => o.HotelId == request.HotelId && o.ProviderName.Equals(request.ProviderName, StringComparison.OrdinalIgnoreCase));

        if (hotel is null || !hotel.IsAvailable)
        {
            throw new InvalidOperationException("Selected hotel is no longer available.");
        }

        var referenceNumber = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
        var reservation = new Reservation(
            referenceNumber,
            request.HotelId,
            request.HotelName,
            request.Destination,
            request.CheckInDate,
            request.CheckOutDate,
            request.RoomType,
            request.GuestName,
            request.GuestDocumentType,
            request.GuestDocumentNumber,
            request.ProviderName,
            hotel.PricePerNight * nights,
            hotel.Currency,
            ReservationStatus.Confirmed,
            hotel.CancellationPolicy,
            DateTime.UtcNow);

        await _reservationRepository.AddAsync(reservation, cancellationToken);
        return reservation;
    }

    public async Task<ReservationLookupResult?> GetReservationAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByReferenceAsync(referenceNumber, cancellationToken);

        return reservation is null
            ? null
            : new ReservationLookupResult(
                reservation.ReferenceNumber,
                reservation.HotelName,
                reservation.Destination,
                reservation.CheckInDate,
                reservation.CheckOutDate,
                reservation.GuestName,
                reservation.ProviderName,
                reservation.TotalPrice,
                reservation.Currency,
                reservation.Status,
                reservation.CancellationPolicy);
    }

}
