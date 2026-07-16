using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Repositories;

public interface IReservationRepository
{
    Task<Reservation?> GetByReferenceAsync(string referenceNumber, CancellationToken cancellationToken = default);
    Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default);
}
