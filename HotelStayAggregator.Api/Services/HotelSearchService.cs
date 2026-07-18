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

        if (string.IsNullOrWhiteSpace(request.RoomType))
        {
            throw new ArgumentException("Room type is required.", nameof(request));
        }

        if (request.CheckOutDate <= request.CheckInDate)
        {
            throw new ArgumentException("Check-out date must be after check-in date.", nameof(request));
        }

        var normalizedRequest = request with { RoomType = NormalizeRoomType(request.RoomType) };
        var searchTasks = _providers.Select(provider => provider.SearchHotelsAsync(normalizedRequest, cancellationToken));
        var providerResults = await Task.WhenAll(searchTasks);

        return providerResults.SelectMany(x => x)
            .Where(offer => offer.IsAvailable)
            .ToList();
    }

    private static string NormalizeRoomType(string roomType)
    {
        var normalized = roomType.Trim().Replace("_", " ");
        if (normalized.Equals("deluxe room", StringComparison.OrdinalIgnoreCase))
        {
            return RoomType.Deluxe.ToString();
        }

        if (normalized.Equals("standard room", StringComparison.OrdinalIgnoreCase))
        {
            return RoomType.Standard.ToString();
        }

        if (normalized.Equals("suite room", StringComparison.OrdinalIgnoreCase))
        {
            return RoomType.Suite.ToString();
        }

        return char.ToUpperInvariant(normalized[0]) + normalized[1..].ToLowerInvariant();
    }
}
