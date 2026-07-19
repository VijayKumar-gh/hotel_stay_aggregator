using HotelStayAggregator.Api.Exceptions;
using HotelStayAggregator.Api.Models;
using HotelStayAggregator.Api.Providers;
using HotelStayAggregator.Api.Repositories;
using HotelStayAggregator.Api.Services;
using HotelStayAggregator.Api.Validation;

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
    public async Task SearchHotelsAsync_NormalizesSnakeCaseRoomTypeAndSupportsPascalCaseInput()
    {
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new HotelSearchService(providers);

        var request = new HotelSearchRequest("Berlin", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3), "deluxe_room");

        var result = await service.SearchHotelsAsync(request);

        Assert.NotEmpty(result);
        Assert.All(result, offer => Assert.Equal("Deluxe", offer.RoomType));
    }

    [Fact]
    public async Task SearchHotelsAsync_FiltersUnavailableRooms_WhenBudgetNestsHasNoInventory()
    {
        var providers = new IHotelProvider[] { new BudgetNestsProvider() };
        var service = new HotelSearchService(providers);

        var request = new HotelSearchRequest("Berlin", new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 3), "deluxe_room");

        var result = await service.SearchHotelsAsync(request);

        Assert.All(result, offer => Assert.True(offer.IsAvailable));
        Assert.DoesNotContain(result, offer => offer.ProviderName == "BudgetNests" && !offer.IsAvailable);
    }

    [Fact]
    public async Task ReserveHotelAsync_CreatesReservationAndLookupReturnsIt()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var providerResolver = new HotelProviderResolver(providers);
        var validator = new ReserveHotelRequestValidator();
        var service = new ReservationService(repository, providerResolver, validator);

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
    public async Task ReserveHotelAsync_RejectsInvalidDocumentType_WithClearValidationMessage()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var providerResolver = new HotelProviderResolver(providers);
        var validator = new ReserveHotelRequestValidator();
        var service = new ReservationService(repository, providerResolver, validator);

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

        var exception = await Assert.ThrowsAsync<ReservationValidationException>(() => service.ReserveHotelAsync(request));

        Assert.Contains("Passport", exception.Message);
    }

    [Fact]
    public async Task ReserveHotelAsync_RejectsDomesticDestinationWithoutNationalId()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var providerResolver = new HotelProviderResolver(providers);
        var validator = new ReserveHotelRequestValidator();
        var service = new ReservationService(repository, providerResolver, validator);

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

        var exception = await Assert.ThrowsAsync<ReservationValidationException>(() => service.ReserveHotelAsync(request));

        Assert.Contains("National ID", exception.Message);
    }

    [Fact]
    public async Task SearchHotelsAsync_RejectsInvalidDateRange()
    {
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var service = new HotelSearchService(providers);

        var request = new HotelSearchRequest("Berlin", new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 3), "Deluxe");

        await Assert.ThrowsAsync<ArgumentException>(() => service.SearchHotelsAsync(request));
    }

    [Fact]
    public async Task ReserveHotelAsync_RejectsMissingGuestName_AsBasicInputValidation()
    {
        var repository = new InMemoryReservationRepository();
        var providers = new IHotelProvider[] { new PremierStaysProvider(), new BudgetNestsProvider() };
        var providerResolver = new HotelProviderResolver(providers);
        var validator = new ReserveHotelRequestValidator();
        var service = new ReservationService(repository, providerResolver, validator);

        var request = new ReserveHotelRequest(
            "PS-101",
            "Ocean View Suites",
            "Berlin",
            new DateOnly(2026, 8, 1),
            new DateOnly(2026, 8, 3),
            "Deluxe",
            string.Empty,
            "Passport",
            "P1234567",
            "PremierStays");

        await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveHotelAsync(request));
    }
}
