# Hotel Stay Aggregator

A .NET 8 Minimal API + React TypeScript sample that aggregates hotel offers from multiple providers, validates reservation input, and returns reservation confirmations by reference number.

## Solution structure

- `HotelStayAggregator.Api` — Minimal API backend
- `HotelStayAggregator.Web` — React + TypeScript frontend
- `HotelStayAggregator.Tests` — xUnit tests

## Prerequisites

Before running the project locally, install:

- .NET 8 SDK
- Node.js 18+ and npm
- A terminal such as PowerShell or Command Prompt

## Run from a clean clone

1. Restore the .NET solution:

   ```powershell
   dotnet restore
   ```

2. Run the API:

   ```powershell
   dotnet run --project .\HotelStayAggregator.Api\HotelStayAggregator.Api.csproj
   ```

   The API will start with Swagger enabled for local exploration.

3. In a second terminal, install and run the React frontend:

   ```powershell
   cd .\HotelStayAggregator.Web
   npm install
   npm run dev
   ```

4. Open the frontend URL shown by Vite, typically `http://localhost:5173`.

## API endpoints

- `GET /api/hotels/search?destination=Berlin&checkInDate=2026-08-01&checkOutDate=2026-08-03&roomType=Deluxe`
- `POST /api/reservations`
- `GET /api/reservations/{referenceNumber}`

## Validation behavior

- Missing required search parameters or an invalid date range returns `400 Bad Request`.
- Document mismatches such as domestic destinations using `Passport` return `422 Unprocessable Entity`.
- Successful reservation flow returns a generated reference number.

## Run tests

```powershell
dotnet test
```

## Notes

The current implementation uses a provider abstraction that makes it easy to add a third provider by implementing `IHotelProvider` and registering it in the existing dependency injection container.
