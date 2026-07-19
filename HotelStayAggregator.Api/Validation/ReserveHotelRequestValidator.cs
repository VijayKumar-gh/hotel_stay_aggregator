using HotelStayAggregator.Api.Exceptions;
using HotelStayAggregator.Api.Models;

namespace HotelStayAggregator.Api.Validation;

public interface IReserveHotelRequestValidator
{
    void Validate(ReserveHotelRequest request);
}

public sealed class ReserveHotelRequestValidator : IReserveHotelRequestValidator
{
    public void Validate(ReserveHotelRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.GuestName))
        {
            throw new ArgumentException("Guest name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.GuestDocumentType))
        {
            throw new ReservationValidationException("Guest document type is required.");
        }

        if (string.IsNullOrWhiteSpace(request.GuestDocumentNumber))
        {
            throw new ReservationValidationException("Guest document number is required.");
        }

        var isDomesticDestination = IsDomesticDestination(request.Destination);
        if (isDomesticDestination)
        {
            if (!request.GuestDocumentType.Equals("National ID", StringComparison.OrdinalIgnoreCase))
            {
                throw new ReservationValidationException("Domestic destinations require National ID.");
            }

            return;
        }

        if (!request.GuestDocumentType.Equals("Passport", StringComparison.OrdinalIgnoreCase))
        {
            throw new ReservationValidationException("International destinations require Passport.");
        }
    }

    private static bool IsDomesticDestination(string destination)
    {
        return destination.Equals("New York", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Seattle", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Boston", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Austin", StringComparison.OrdinalIgnoreCase)
            || destination.Equals("Miami", StringComparison.OrdinalIgnoreCase);
    }
}
