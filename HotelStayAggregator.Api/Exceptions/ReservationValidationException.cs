namespace HotelStayAggregator.Api.Exceptions;

public sealed class ReservationValidationException : ArgumentException
{
    public ReservationValidationException(string message)
        : base(message)
    {
    }
}
