# Copilot Prompt Examples

## Scaffold a new provider

Create a new provider class for the Hotel Stay Aggregator solution that implements `IHotelProvider`, returns mock hotel offers for a given destination and room type, and is registered in `Program.cs` with dependency injection.

## Add business logic for normalization

Update the search service so input variants like `snake_case`, `PascalCase`, and `camelCase` are normalized before provider queries, and keep the returned `RoomType` consistent across all providers.

## Add tests for validation

Add xUnit tests for the reservation flow that verify document validation errors use clear messages and that missing or invalid input returns the expected result status from the API layer.

## Generate a frontend reservation experience

Create a React component that lets a user search hotels, choose one offer, submit a reservation form, and display a friendly confirmation step showing the reference number, provider name, total price, and cancellation policy.

## Improve API contracts

Update the Minimal API so semantic validation failures return `422 Unprocessable Entity` with an error payload, while true request-shape problems return `400 Bad Request`.