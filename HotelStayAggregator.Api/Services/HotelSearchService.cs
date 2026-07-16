using HotelStayAggregator.Api.Models;
using HotelStayAggregator.Api.Providers;

namespace HotelStayAggregator.Api.Services;

public sealed class HotelSearchService : IHotelSearchService
{
    private readonly IEnumerable<IHotelProvider> _providers;

    public HotelSearchService(IEnumerable<IHotelProvider> providers)
    {
        _providers = providers;
    }

    public async Task<IReadOnlyList<HotelOffer>> SearchHotelsAsync(HotelSearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            throw new ArgumentException("Destination is required.", nameof(request));
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date.", nameof(request));
        }

        var searchTasks = _providers.Select(provider => provider.SearchHotelsAsync(request, cancellationToken));
        var providerResults = await Task.WhenAll(searchTasks);

        return providerResults.SelectMany(x => x).ToList();
    }
}
