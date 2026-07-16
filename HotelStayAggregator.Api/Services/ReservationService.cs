using HotelStayAggregator.Api.Models;
using HotelStayAggregator.Api.Providers;
using HotelStayAggregator.Api.Repositories;

namespace HotelStayAggregator.Api.Services;

public interface IReservationService
{
    Task<Reservation> ReserveHotelAsync(ReserveHotelRequest request, CancellationToken cancellationToken = default);
    Task<ReservationLookupResult?> GetReservationAsync(string referenceNumber, CancellationToken cancellationToken = default);
}

public sealed class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IEnumerable<IHotelProvider> _providers;

    public ReservationService(IReservationRepository reservationRepository, IEnumerable<IHotelProvider> providers)
    {
        _reservationRepository = reservationRepository;
        _providers = providers;
    }

    public async Task<Reservation> ReserveHotelAsync(ReserveHotelRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var provider = _providers.FirstOrDefault(p => p.ProviderName.Equals(request.ProviderName, StringComparison.OrdinalIgnoreCase));
        if (provider is null)
        {
            throw new ArgumentException($"Unsupported provider '{request.ProviderName}'.");
        }

        var nights = (request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber);
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
                reservation.Currency);
    }

    private static void ValidateRequest(ReserveHotelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            throw new ArgumentException("Guest name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.GuestDocumentType))
        {
            throw new ArgumentException("Guest document type is required.");
        }

        if (string.IsNullOrWhiteSpace(request.GuestDocumentNumber))
        {
            throw new ArgumentException("Guest document number is required.");
        }

        var isDomesticDestination = IsDomesticDestination(request.Destination);
        if (isDomesticDestination)
        {
            if (!request.GuestDocumentType.Equals("National ID", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Domestic destinations require National ID.");
            }

            return;
        }

        if (!request.GuestDocumentType.Equals("Passport", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("International destinations require Passport.");
        }
    }

    private static bool IsDomesticDestination(string destination)
    {
        return destination.Equals("New York", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Seattle", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Boston", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Austin", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Miami", StringComparison.OrdinalIgnoreCase);
    }
}
