using Microsoft.AspNetCore.Mvc;
using FlightBooking.Core.Interfaces;
using FlightBooking.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FlightBooking.Web.Controllers.Api;

[ApiController]
[Route("api/bookings")]
public class BookingsApiController : ControllerBase
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IUserRepository _userRepository;
    private readonly IPassengerRepository _passengerRepository;
    private readonly ILogger<BookingsApiController> _logger;

    public BookingsApiController(
        IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IUserRepository userRepository,
        IPassengerRepository passengerRepository,
        ILogger<BookingsApiController> logger)
    {
        _bookingRepository = bookingRepository;
        _flightRepository = flightRepository;
        _userRepository = userRepository;
        _passengerRepository = passengerRepository;
        _logger = logger;
    }

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllBookings()
    {
        try
        {
            var bookings = await _bookingRepository.GetAllAsync();
            
            var result = new List<object>();
            foreach (var booking in bookings)
            {
                var user = await _userRepository.GetByIdAsync(booking.UserId);
                var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                
                result.Add(new
                {
                    id = booking.Id,
                    bookingReference = booking.BookingReference,
                    flightId = booking.FlightId,
                    userId = booking.UserId,
                    passengerCount = booking.PassengerCount,
                    totalPrice = booking.TotalPrice,
                    status = booking.Status,
                    isPaid = booking.IsPaid,
                    bookingDate = booking.BookingDate,
                    user = user != null ? new { email = user.Email } : null,
                    flight = flight != null ? new { flightNumber = flight.FlightNumber } : null
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all bookings");
            return StatusCode(500, "Rezervasyonlar yüklenirken hata oluştu");
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(int id)
    {
        try
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound("Rezervasyon bulunamadı");
            }

            if (booking.Status == "Cancelled")
            {
                return BadRequest("Rezervasyon zaten iptal edilmiş");
            }

            // Update booking status
            booking.Status = "Cancelled";

            // Return seats to flight
            var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
            if (flight != null)
            {
                flight.AvailableSeats += booking.PassengerCount;
                await _flightRepository.UpdateAsync(flight);
            }

            await _bookingRepository.UpdateAsync(booking);
            return Ok(new { message = "Rezervasyon başarıyla iptal edildi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
            return StatusCode(500, "Rezervasyon iptal edilirken hata oluştu");
        }
    }

    [HttpDelete("all")]
    [Authorize]
    public async Task<IActionResult> DeleteAllBookings()
    {
        try
        {
            var bookings = await _bookingRepository.GetAllAsync();
            var deletedCount = 0;
            var deletedPassengers = 0;

            foreach (var booking in bookings)
            {
                // Return seats to flight
                var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                if (flight != null)
                {
                    flight.AvailableSeats += booking.PassengerCount;
                    await _flightRepository.UpdateAsync(flight);
                }

                deletedPassengers += booking.PassengerCount;
                await _bookingRepository.DeleteAsync(booking);
                deletedCount++;
            }

            return Ok(new { 
                deletedCount = deletedCount, 
                deletedPassengers = deletedPassengers,
                message = "Tüm rezervasyonlar başarıyla silindi"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all bookings");
            return StatusCode(500, "Toplu silme işlemi sırasında hata oluştu");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
    {
        try
        {
            var flight = await _flightRepository.GetByIdAsync(request.FlightId);
            if (flight == null)
            {
                return NotFound("Uçuş bulunamadı");
            }

            if (flight.AvailableSeats < request.Passengers.Count)
            {
                return BadRequest("Yeterli koltuk bulunmamaktadır");
            }

            // Create or get user
            User user;
            if (!string.IsNullOrEmpty(request.GuestEmail))
            {
                user = await _userRepository.GetByEmailAsync(request.GuestEmail);
                if (user == null)
                {
                    user = new User
                    {
                        Email = request.GuestEmail,
                        IsGuest = true,
                        IsAdmin = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _userRepository.AddAsync(user);
                }
            }
            else
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Unauthorized("Kullanıcı bulunamadı");
                }
            }

            // Create booking
            var booking = new Booking
            {
                FlightId = request.FlightId,
                UserId = user.Id,
                BookingReference = GeneratePNR(),
                PassengerCount = request.Passengers.Count,
                TotalPrice = flight.Price * request.Passengers.Count,
                Status = "Onaylandı",
                IsPaid = false,
                BookingDate = DateTime.UtcNow
            };

            await _bookingRepository.AddAsync(booking);

            // Create passengers
            foreach (var passengerRequest in request.Passengers)
            {
                var passenger = new Passenger
                {
                    BookingId = booking.Id,
                    FirstName = passengerRequest.FirstName,
                    LastName = passengerRequest.LastName,
                    IdentityNumber = passengerRequest.IdentityNumber,
                    PhoneNumber = passengerRequest.PhoneNumber,
                    DateOfBirth = passengerRequest.DateOfBirth,
                    Gender = passengerRequest.Gender,
                    SeatNumber = passengerRequest.SeatNumber ?? "TBD",
                    SeatType = passengerRequest.SeatType ?? "Economy"
                };

                await _passengerRepository.AddAsync(passenger);
            }

            // Update flight available seats
            flight.AvailableSeats -= request.Passengers.Count;
            await _flightRepository.UpdateAsync(flight);

            return Ok(new { 
                bookingReference = booking.BookingReference,
                message = "Rezervasyon başarıyla oluşturuldu"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return StatusCode(500, "Rezervasyon oluşturulurken hata oluştu");
        }
    }

    private string GeneratePNR()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}

public class CreateBookingRequest
{
    public int FlightId { get; set; }
    public string? GuestEmail { get; set; }
    public List<PassengerRequest> Passengers { get; set; } = new();
}

public class PassengerRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
}