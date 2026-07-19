using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Providers;

public interface IHotelProviderResolver
{
    IHotelProvider? Resolve(string providerName);
}

public sealed class HotelProviderResolver : IHotelProviderResolver
{
    private readonly IReadOnlyDictionary<string, IHotelProvider> _providers;

    public HotelProviderResolver(IEnumerable<IHotelProvider> providers)
    {
        _providers = providers
            .GroupBy(provider => provider.ProviderName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public IHotelProvider? Resolve(string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return null;
        }

        return _providers.TryGetValue(providerName, out var provider)
            ? provider
            : null;
    }
}
