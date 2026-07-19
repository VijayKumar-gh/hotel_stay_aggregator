# Hotel Stay Aggregator Specification

## Domain overview

The solution aggregates hotel offers from multiple providers, validates reservation input, and returns a reservation reference number for confirmation and lookup.

## Core domain models

### RoomType enum

```csharp
public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}
```

### HotelRoom record

```csharp
public record HotelRoom(
    string HotelId,
    string HotelName,
    string Destination,
    RoomType RoomType,
    decimal PricePerNight,
    string Currency,
    string ProviderName,
    bool IsAvailable);
```

### Reservation record

```csharp
public record Reservation(
    string ReferenceNumber,
    string HotelId,
    string HotelName,
    string Destination,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string RoomType,
    string GuestName,
    string GuestDocumentType,
    string GuestDocumentNumber,
    string ProviderName,
    decimal TotalPrice,
    string Currency,
    DateTime CreatedAtUtc);
```

## Interface contract

```csharp
public interface IHotelProvider
{
    string ProviderName { get; }

    Task<IReadOnlyList<HotelOffer>> SearchHotelsAsync(
        HotelSearchRequest request,
        CancellationToken cancellationToken = default);
}
```

## Search and reservation request contracts

### HotelSearchRequest

```csharp
public record HotelSearchRequest(
    string Destination,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string RoomType);
```

### ReserveHotelRequest

```csharp
public record ReserveHotelRequest(
    string HotelId,
    string HotelName,
    string Destination,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string RoomType,
    string GuestName,
    string GuestDocumentType,
    string GuestDocumentNumber,
    string ProviderName);
```

## Validation rules

- Missing destination, check-in, check-out, or room type returns `400 Bad Request`.
- Invalid date ranges return `400 Bad Request`.
- Domestic destinations require `National ID`.
- International destinations require `Passport`.
- Document mismatches return `422 Unprocessable Entity` with a clear message.
- Successful reservation returns a generated reference number and confirmation payload.

## Extensibility

The provider abstraction is intentionally small:

- Add a new provider class.
- Implement `IHotelProvider`.
- Register it in the DI container in [HotelStayAggregator.Api/Program.cs](HotelStayAggregator.Api/Program.cs).

This keeps the solution open for a third provider without changing the search or reservation service contracts.
