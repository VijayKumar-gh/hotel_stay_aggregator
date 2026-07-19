using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Providers;

public sealed class BudgetNestsProvider : IHotelProvider
{
    public string ProviderName => "BudgetNests";

    public Task<IReadOnlyList<HotelOffer>> SearchHotelsAsync(HotelSearchRequest request, CancellationToken cancellationToken = default)
    {
        var hotels = new List<HotelOffer>
        {
            new("BN-80", "City Stay Inn", request.Destination, request.RoomType, 120m, "USD", ProviderName, true, "Free cancellation up to 24 hours before check-in."),
            new("BN-140", "Comfort Point", request.Destination, request.RoomType, 145m, "USD", ProviderName, true, "Free cancellation up to 24 hours before check-in.")
        };

        return Task.FromResult<IReadOnlyList<HotelOffer>>(hotels);
    }
}
