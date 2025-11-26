using FlightBooking.Application.DTOs;
using FlightBooking.Core.Entities;
using FlightBooking.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlightBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingRequest request)
    {
        var flight = await _context.Flights
            .Include(f => f.Bookings)
            .ThenInclude(b => b.Passengers)
            .FirstOrDefaultAsync(f => f.Id == request.FlightId);
            
        if (flight == null || flight.AvailableSeats < request.Passengers.Count)
            return BadRequest(new { message = "Uçuş bulunamadı veya yeterli koltuk yok" });

        // Seçilen koltukların dolu olup olmadığını kontrol et
        var occupiedSeats = flight.Bookings
            .Where(b => b.Status == "Confirmed" || b.Status == "Pending")
            .SelectMany(b => b.Passengers)
            .Where(p => !string.IsNullOrEmpty(p.SeatNumber))
            .Select(p => p.SeatNumber)
            .ToList();

        var requestedSeats = request.Passengers
            .Where(p => !string.IsNullOrEmpty(p.SeatNumber))
            .Select(p => p.SeatNumber)
            .ToList();

        var conflictingSeats = requestedSeats.Intersect(occupiedSeats).ToList();
        if (conflictingSeats.Any())
            return BadRequest(new { message = $"Seçilen koltuklar dolu: {string.Join(", ", conflictingSeats)}" });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var booking = new Booking
        {
            BookingReference = GenerateBookingReference(),
            UserId = userId,
            FlightId = request.FlightId,
            PassengerCount = request.Passengers.Count,
            TotalPrice = flight.Price * request.Passengers.Count,
            Status = "Confirmed",
            BookingDate = DateTime.UtcNow,
            IsPaid = false
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        foreach (var passengerDto in request.Passengers)
        {
            var passenger = new Passenger
            {
                BookingId = booking.Id,
                FirstName = passengerDto.FirstName,
                LastName = passengerDto.LastName,
                IdentityNumber = passengerDto.IdentityNumber,
                DateOfBirth = passengerDto.DateOfBirth,
                Gender = passengerDto.Gender,
                SeatNumber = passengerDto.SeatNumber ?? string.Empty,
                SeatType = passengerDto.SeatType ?? string.Empty
            };
            _context.Passengers.Add(passenger);
        }

        flight.AvailableSeats -= request.Passengers.Count;
        await _context.SaveChangesAsync();

        return Ok(booking);
    }

    [HttpGet]
    public async Task<IActionResult> GetMyBookings()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var bookings = await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBooking(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var isAdmin = User.IsInRole("Admin");

        var booking = await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound();

        if (!isAdmin && booking.UserId != userId)
            return Forbid();

        return Ok(booking);
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> PayBooking(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null)
            return NotFound();

        if (booking.UserId != userId)
            return Forbid();

        if (booking.IsPaid)
            return BadRequest(new { message = "Rezervasyon zaten ödenmiş" });

        booking.IsPaid = true;
        booking.PaymentDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(booking);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized();

        var userId = int.Parse(userIdClaim);
        var isAdmin = User.IsInRole("Admin");

        var booking = await _context.Bookings
            .Include(b => b.Flight)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null)
            return NotFound();

        if (!isAdmin && booking.UserId != userId)
            return Forbid();

        var hoursDifference = (booking.Flight.DepartureTime - DateTime.UtcNow).TotalHours;
        if (hoursDifference < 24)
            return BadRequest(new { message = "Uçuştan 24 saat öncesine kadar iptal yapabilirsiniz" });

        booking.Status = "Cancelled";
        booking.Flight.AvailableSeats += booking.PassengerCount;
        
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public async Task<IActionResult> GetAllBookings()
    {
        var bookings = await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.User)
            .Include(b => b.Passengers)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();

        return Ok(bookings);
    }

    [AllowAnonymous]
    [HttpGet("pnr/{pnr}")]
    public async Task<IActionResult> GetBookingByPNR(string pnr, [FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "E-posta adresi gereklidir" });

        var normalizedEmail = email.ToLower().Trim();
        var normalizedPnr = pnr.ToUpper().Trim();

        var booking = await _context.Bookings
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.BookingReference == normalizedPnr && b.User.Email.ToLower() == normalizedEmail);

        if (booking == null)
            return NotFound(new { message = "PNR numarası veya e-posta adresi hatalı" });

        return Ok(booking);
    }

    [AllowAnonymous]
    [HttpPost("pnr/{pnr}/pay")]
    public async Task<IActionResult> PayBookingByPNR(string pnr, [FromQuery] string email)
    {
        if (string.IsNullOrEmpty(email))
            return BadRequest(new { message = "E-posta adresi gereklidir" });

        var normalizedEmail = email.ToLower().Trim();
        var normalizedPnr = pnr.ToUpper().Trim();

        var booking = await _context.Bookings
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.BookingReference == normalizedPnr && b.User.Email.ToLower() == normalizedEmail);

        if (booking == null)
            return NotFound(new { message = "PNR numarası veya e-posta adresi hatalı" });

        if (booking.IsPaid)
            return BadRequest(new { message = "Rezervasyon zaten ödenmiş" });

        booking.IsPaid = true;
        booking.PaymentDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Ok(booking);
    }

    private string GenerateBookingReference()
    {
        // PNR formatı: 6 karakterli benzersiz kod (örn: ABC123)
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var pnr = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        return pnr;
    }
}
