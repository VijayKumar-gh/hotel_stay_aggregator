# Hotel Stay Aggregator Specification

## Features

### Search Hotels

Users can search hotels using:

- Destination
- Check-In Date
- Check-Out Date
- Room Type

### Reserve Hotel

Users can reserve a room.

## Reservation Process
the user shall be navigated to a Reservation
Form.
from the search results,
After selecting a hotel
The Reservation Form shall capture:
Guest Name Document Type Passport National ID Document Number
Business Rules:
International destinations require Passport. Domestic destinations allow National ID. Invalid documents return validation errors.
Upon successful reservation:
Generate Reservation Reference Number
Display Confirmation Page

### View Reservation

Users can retrieve reservation information using a reference number.

## Business Rules

- International destinations require Passport.
- Domestic destinations allow National ID.
- Invalid dates return Bad Request.


## Domain overview

This solution aggregates hotel offers from multiple providers and lets a user reserve and retrieve a reservation by reference number.

## Core models

### RoomType enum

```csharp
public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}
```

### HotelRoom model

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

### Reservation model

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

### Provider responsibilities

- Return hotel offers for the supplied search request.
- Return provider-specific `ProviderName` metadata.
- Mark unavailable inventory with `IsAvailable = false`.
- Remain easy to add by registering additional providers in dependency injection.

## Search and reservation contracts

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

- Missing destination, room type, check-in, or check-out values should return `400`.
- Invalid date ranges should return `400`.
- Domestic destinations require `National ID`.
- International destinations require `Passport`.
- Document mismatches should return `422` with a clear error message.
- Successful reservation returns a generated reference number and confirmation payload.

## Provider extensibility

The provider abstraction is intentionally small:

- Add a new provider class.
- Implement `IHotelProvider`.
- Register it in `Program.cs` with dependency injection.

That supports a clean path to add a third provider without touching the search or reservation business logic.
