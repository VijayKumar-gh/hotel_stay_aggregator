using HotelStayAggregator.Api.Exceptions;
using HotelStayAggregator.Api.Models;
using HotelStayAggregator.Api.Providers;
using HotelStayAggregator.Api.Repositories;
using HotelStayAggregator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddSingleton<IHotelProvider, PremierStaysProvider>();
builder.Services.AddSingleton<IHotelProvider, BudgetNestsProvider>();
builder.Services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
builder.Services.AddScoped<IHotelSearchService, HotelSearchService>();
builder.Services.AddScoped<IReservationService, ReservationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.MapGet("/api/hotels/search", async (string destination, DateOnly checkInDate, DateOnly checkOutDate, string roomType, IHotelSearchService hotelSearchService, CancellationToken cancellationToken) =>
{
    try
    {
        var request = new HotelSearchRequest(destination, checkInDate, checkOutDate, roomType);
        return Results.Ok(await hotelSearchService.SearchHotelsAsync(request, cancellationToken));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
})
.WithName("SearchHotels")
.WithOpenApi();

app.MapPost("/api/reservations", async (ReserveHotelRequest request, IReservationService reservationService, CancellationToken cancellationToken) =>
{
    try
    {
        var reservation = await reservationService.ReserveHotelAsync(request, cancellationToken);
        return Results.Created($"/api/reservations/{reservation.ReferenceNumber}", reservation);
    }
    catch (ReservationValidationException ex)
    {
        return Results.UnprocessableEntity(new { error = ex.Message });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return Results.Conflict(new { error = ex.Message });
    }
})
.WithName("ReserveHotel")
.WithOpenApi();

app.MapGet("/api/reservations/{referenceNumber}", async (string referenceNumber, IReservationService reservationService, CancellationToken cancellationToken) =>
{
    var reservation = await reservationService.GetReservationAsync(referenceNumber, cancellationToken);
    return reservation is null
        ? Results.NotFound()
        : Results.Ok(reservation);
})
.WithName("GetReservation")
.WithOpenApi();

app.Run();
