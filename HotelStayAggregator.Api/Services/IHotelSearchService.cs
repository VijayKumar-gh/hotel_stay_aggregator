using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Services;

public interface IHotelSearchService
{
    Task<IReadOnlyList<HotelOffer>> SearchHotelsAsync(HotelSearchRequest request, CancellationToken cancellationToken = default);
}
