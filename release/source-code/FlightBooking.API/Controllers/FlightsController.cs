using FlightBooking.Application.DTOs;
using FlightBooking.Core.Entities;
using FlightBooking.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FlightsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> SearchFlights([FromQuery] FlightSearchRequest request)
    {
        var query = _context.Flights.Where(f => f.Status == "Active" && f.AvailableSeats > 0);

        if (!string.IsNullOrEmpty(request.DepartureCity))
            query = query.Where(f => f.DepartureCity.Contains(request.DepartureCity));

        if (!string.IsNullOrEmpty(request.ArrivalCity))
            query = query.Where(f => f.ArrivalCity.Contains(request.ArrivalCity));

        if (request.DepartureDate.HasValue)
            query = query.Where(f => f.DepartureTime.Date == request.DepartureDate.Value.Date);

        if (request.MinPrice.HasValue)
            query = query.Where(f => f.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(f => f.Price <= request.MaxPrice.Value);

        var flights = await query.OrderBy(f => f.DepartureTime).ToListAsync();
        return Ok(flights);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetFlight(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null)
            return NotFound();

        return Ok(flight);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateFlight([FromBody] Flight flight)
    {
        flight.CreatedAt = DateTime.UtcNow;
        flight.AvailableSeats = flight.TotalSeats;
        
        _context.Flights.Add(flight);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetFlight), new { id = flight.Id }, flight);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFlight(int id, [FromBody] Flight flight)
    {
        var existingFlight = await _context.Flights.FindAsync(id);
        if (existingFlight == null)
            return NotFound();

        existingFlight.FlightNumber = flight.FlightNumber;
        existingFlight.Airline = flight.Airline;
        existingFlight.DepartureCity = flight.DepartureCity;
        existingFlight.ArrivalCity = flight.ArrivalCity;
        existingFlight.DepartureTime = flight.DepartureTime;
        existingFlight.ArrivalTime = flight.ArrivalTime;
        existingFlight.Price = flight.Price;
        existingFlight.TotalSeats = flight.TotalSeats;
        existingFlight.Status = flight.Status;

        await _context.SaveChangesAsync();
        return Ok(existingFlight);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFlight(int id)
    {
        var flight = await _context.Flights.FindAsync(id);
        if (flight == null)
            return NotFound();

        flight.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/seats")]
    public async Task<IActionResult> GetSeatMap(int id)
    {
        var flight = await _context.Flights
            .Include(f => f.Bookings)
            .ThenInclude(b => b.Passengers)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (flight == null)
            return NotFound();

        // Dolu koltukları topla
        var occupiedSeats = flight.Bookings
            .Where(b => b.Status == "Confirmed" || b.Status == "Pending")
            .SelectMany(b => b.Passengers)
            .Where(p => !string.IsNullOrEmpty(p.SeatNumber))
            .Select(p => p.SeatNumber)
            .ToList();

        // Koltuk haritası oluştur
        var seatMap = new
        {
            flight.SeatsPerRow,
            flight.TotalRows,
            OccupiedSeats = occupiedSeats,
            SeatLayout = GenerateSeatLayout(flight.SeatsPerRow, flight.TotalRows, occupiedSeats)
        };

        return Ok(seatMap);
    }

    private List<object> GenerateSeatLayout(int seatsPerRow, int totalRows, List<string> occupiedSeats)
    {
        var layout = new List<object>();
        var seatLetters = new[] { "A", "B", "C", "D", "E", "F" };

        for (int row = 1; row <= totalRows; row++)
        {
            var rowSeats = new List<object>();
            for (int col = 0; col < Math.Min(seatsPerRow, seatLetters.Length); col++)
            {
                var seatNumber = $"{row}{seatLetters[col]}";
                var seatType = (col == 0 || col == seatsPerRow - 1) ? "Window" : 
                               (col == 1 || col == seatsPerRow - 2) ? "Aisle" : "Middle";
                
                rowSeats.Add(new
                {
                    SeatNumber = seatNumber,
                    SeatType = seatType,
                    IsOccupied = occupiedSeats.Contains(seatNumber),
                    Row = row,
                    Column = seatLetters[col]
                });
            }
            layout.Add(new { Row = row, Seats = rowSeats });
        }

        return layout;
    }
}
