using HotelStayAggregator.Api.Models;
using HotelStayAggregator.Api.Providers;
using HotelStayAggregator.Api.Repositories;
using HotelStayAggregator.Api.Services;

namespace HotelStayAggregator.Tests;

public class HotelStayAggregatorTests
{
    [Fact]
    public async Task SearchHotelsAsync_ReturnsOffersFromAllProviders()
    {
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new HotelSearchService(providers);

        var request = new HotelSearchRequest("Berlin", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3), "Deluxe");

        var result = await service.SearchHotelsAsync(request);

        Assert.Equal(4, result.Count);
        Assert.Contains(result, offer => offer.ProviderName == "PremierStays");
        Assert.Contains(result, offer => offer.ProviderName == "BudgetNests");
    }

    [Fact]
    public async Task ReserveHotelAsync_CreatesReservationAndLookupReturnsIt()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new ReservationService(repository, providers);

        var request = new ReserveHotelRequest(
            "PS-101",
            "Ocean View Suites",
            "Berlin",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 3),
            "Deluxe",
            "Ada Lovelace",
            "Passport",
            "P1234567",
            "PremierStays");

        var reservation = await service.ReserveHotelAsync(request);
        var lookup = await service.GetReservationAsync(reservation.ReferenceNumber);

        Assert.NotNull(lookup);
        Assert.Equal(reservation.ReferenceNumber, lookup!.ReferenceNumber);
        Assert.Equal("Ocean View Suites", lookup.HotelName);
        Assert.Equal(370m, lookup.TotalPrice);
    }

    [Fact]
    public async Task ReserveHotelAsync_RejectsInvalidDocumentType()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new ReservationService(repository, providers);

        var request = new ReserveHotelRequest(
            "PS-101",
            "Ocean View Suites",
            "Berlin",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 3),
            "Deluxe",
            "Ada Lovelace",
            "Driver License",
            "P1234567",
            "PremierStays");

        await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveHotelAsync(request));
    }

    [Fact]
    public async Task ReserveHotelAsync_RejectsDomesticDestinationWithoutNationalId()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new ReservationService(repository, providers);

        var request = new ReserveHotelRequest(
            "PS-101",
            "Ocean View Suites",
            "Seattle",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 3),
            "Deluxe",
            "Ada Lovelace",
            "Passport",
            "P1234567",
            "PremierStays");

        await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveHotelAsync(request));
    }

    [Fact]
    public async Task SearchHotelsAsync_RejectsInvalidDateRange()
    {
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new HotelSearchService(providers);

        var request = new HotelSearchRequest("Berlin", new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 3), "Deluxe");

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchHotelsAsync(request));
    }
}
