using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Providers;

public sealed class PremierStaysProvider : IHotelProvider
{
    public string ProviderName => "PremierStays";

    public Task<IReadOnlyList<HotelOffer>> SearchHotelsAsync(HotelSearchRequest request, CancellationToken cancellationToken = default)
    {
        var hotels = new List<HotelOffer>
        {
            new("PS-101", "Ocean View Suites", request.Destination, request.RoomType, 185m, "USD", ProviderName, true),
            new("PS-205", "Harbor Luxe", request.Destination, request.RoomType, 220m, "USD", ProviderName, true)
        };

        return Task.FromResult<IReadOnlyList<HotelOffer>>(hotels);
    }
}
