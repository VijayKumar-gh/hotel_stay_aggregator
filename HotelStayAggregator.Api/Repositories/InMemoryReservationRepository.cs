using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Repositories;

public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly Dictionary<string, Reservation> _reservations = new(StringComparer.OrdinalIgnoreCase);

    public Task<Reservation?> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_reservations.TryGetValue(referenceNumber, out var reservation) ? reservation : null);
    }

    public Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _reservations[reservation.ReferenceNumber] = reservation;
        return Task.CompletedTask;
    }
}
