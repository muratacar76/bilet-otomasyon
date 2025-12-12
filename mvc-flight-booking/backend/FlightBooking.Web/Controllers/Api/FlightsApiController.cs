using Microsoft.AspNetCore.Mvc;
using FlightBooking.Core.Interfaces;
using FlightBooking.Core.Entities;
using Microsoft.AspNetCore.Authorization;

namespace FlightBooking.Web.Controllers.Api;

[ApiController]
[Route("api/flights")]
public class FlightsApiController : ControllerBase
{
    private readonly IFlightRepository _flightRepository;
    private readonly ILogger<FlightsApiController> _logger;

    public FlightsApiController(IFlightRepository flightRepository, ILogger<FlightsApiController> logger)
    {
        _flightRepository = flightRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetFlights([FromQuery] string? departureCity = null, 
                                               [FromQuery] string? arrivalCity = null, 
                                               [FromQuery] DateTime? departureDate = null)
    {
        try
        {
            var flights = await _flightRepository.GetAllAsync();
            
            // Apply filters
            if (!string.IsNullOrEmpty(departureCity))
            {
                flights = flights.Where(f => f.DepartureCity.Contains(departureCity, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!string.IsNullOrEmpty(arrivalCity))
            {
                flights = flights.Where(f => f.ArrivalCity.Contains(arrivalCity, StringComparison.OrdinalIgnoreCase));
            }
            
            if (departureDate.HasValue)
            {
                var date = departureDate.Value.Date;
                flights = flights.Where(f => f.DepartureTime.Date == date);
            }

            var result = flights.Select(f => new
            {
                id = f.Id,
                flightNumber = f.FlightNumber,
                airline = f.Airline,
                departureCity = f.DepartureCity,
                arrivalCity = f.ArrivalCity,
                departureTime = f.DepartureTime,
                arrivalTime = f.ArrivalTime,
                price = f.Price,
                totalSeats = f.TotalSeats,
                availableSeats = f.AvailableSeats
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting flights");
            return StatusCode(500, "Uçuşlar yüklenirken hata oluştu");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateFlight([FromBody] CreateFlightRequest request)
    {
        try
        {
            var flight = new Flight
            {
                FlightNumber = request.FlightNumber,
                Airline = request.Airline,
                DepartureCity = request.DepartureCity,
                ArrivalCity = request.ArrivalCity,
                DepartureTime = request.DepartureTime,
                ArrivalTime = request.ArrivalTime,
                Price = request.Price,
                TotalSeats = request.TotalSeats,
                AvailableSeats = request.TotalSeats,
                CreatedAt = DateTime.UtcNow
            };

            await _flightRepository.AddAsync(flight);
            return Ok(new { message = "Uçuş başarıyla eklendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating flight");
            return StatusCode(500, "Uçuş eklenirken hata oluştu");
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateFlight(int id, [FromBody] CreateFlightRequest request)
    {
        try
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null)
            {
                return NotFound("Uçuş bulunamadı");
            }

            flight.FlightNumber = request.FlightNumber;
            flight.Airline = request.Airline;
            flight.DepartureCity = request.DepartureCity;
            flight.ArrivalCity = request.ArrivalCity;
            flight.DepartureTime = request.DepartureTime;
            flight.ArrivalTime = request.ArrivalTime;
            flight.Price = request.Price;
            
            // Update available seats if total seats changed
            var seatDifference = request.TotalSeats - flight.TotalSeats;
            flight.TotalSeats = request.TotalSeats;
            flight.AvailableSeats += seatDifference;
            
            flight.UpdatedAt = DateTime.UtcNow;

            await _flightRepository.UpdateAsync(flight);
            return Ok(new { message = "Uçuş başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating flight {FlightId}", id);
            return StatusCode(500, "Uçuş güncellenirken hata oluştu");
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteFlight(int id)
    {
        try
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null)
            {
                return NotFound("Uçuş bulunamadı");
            }

            await _flightRepository.DeleteAsync(flight);
            return Ok(new { message = "Uçuş başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting flight {FlightId}", id);
            return StatusCode(500, "Uçuş silinirken hata oluştu");
        }
    }
}

public class CreateFlightRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
}