namespace HotelStayAggregator.Api.Models;

public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}

public record HotelSearchRequest(
    string Destination,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string RoomType);

public record HotelOffer(
    string HotelId,
    string HotelName,
    string Destination,
    string RoomType,
    decimal PricePerNight,
    string Currency,
    string ProviderName,
    bool IsAvailable);

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

public record ReservationLookupResult(
    string ReferenceNumber,
    string HotelName,
    string Destination,
    DateOnly CheckInDate,
    DateOnly CheckOutDate,
    string GuestName,
    string ProviderName,
    decimal TotalPrice,
    string Currency);
