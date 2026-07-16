using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Providers;

public interface IHotelProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<HotelOffer>> SearchHotelsAsync(HotelSearchRequest request, CancellationToken cancellationToken = default);
}
