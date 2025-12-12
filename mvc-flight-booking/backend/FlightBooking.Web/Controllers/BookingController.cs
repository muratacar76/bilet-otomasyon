using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FlightBooking.Core.Interfaces;
using FlightBooking.Core.Entities;
using FlightBooking.Infrastructure.Services;
using FlightBooking.Application.Services;
using System.Security.Claims;

namespace FlightBooking.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPassengerRepository _passengerRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            IUserRepository userRepository,
            IPassengerRepository passengerRepository,
            IEmailService emailService,
            ILogger<BookingController> logger)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _userRepository = userRepository;
            _passengerRepository = passengerRepository;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Query()
        {
            ViewData["Title"] = "PNR Sorgula - BULUTBİLET.COM";
            return View();
        }

        [HttpGet]
        public IActionResult Manage()
        {
            ViewData["Title"] = "PNR Sorgula - BULUTBİLET.COM";
            return View("Query");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                var bookings = await _bookingRepository.GetByUserIdAsync(userId);
                
                var bookingViewModels = new List<object>();
                foreach (var booking in bookings)
                {
                    var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                    var passengers = await _passengerRepository.GetByBookingIdAsync(booking.Id);
                    
                    bookingViewModels.Add(new
                    {
                        Id = booking.Id,
                        BookingReference = booking.BookingReference,
                        Flight = flight,
                        Passengers = passengers,
                        TotalPrice = booking.TotalPrice,
                        Status = booking.Status,
                        IsPaid = booking.IsPaid,
                        BookingDate = booking.BookingDate,
                        CanBeCancelled = booking.CanBeCancelled
                    });
                }

                ViewData["Title"] = "Rezervasyonlarım - BULUTBİLET.COM";
                return View(bookingViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user bookings");
                TempData["Error"] = "Rezervasyonlar yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchBooking(string pnr, string email)
        {
            try
            {
                if (string.IsNullOrEmpty(pnr) || string.IsNullOrEmpty(email))
                {
                    return Json(new { success = false, message = "PNR ve e-posta adresi gereklidir." });
                }

                var booking = await _bookingRepository.GetByPnrAndEmailAsync(pnr, email);
                
                if (booking == null)
                {
                    return Json(new { success = false, message = "Rezervasyon bulunamadı. PNR ve e-posta adresini kontrol edin." });
                }

                var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                var passengers = await _passengerRepository.GetByBookingIdAsync(booking.Id);

                var result = new
                {
                    success = true,
                    booking = new
                    {
                        booking.Id,
                        booking.BookingReference,
                        booking.Status,
                        booking.TotalPrice,
                        booking.BookingDate,
                        booking.IsPaid,
                        booking.PaymentDate,
                        PassengerCount = passengers.Count()
                    },
                    flight = new
                    {
                        flight.FlightNumber,
                        flight.Airline,
                        flight.DepartureCity,
                        flight.ArrivalCity,
                        DepartureTime = flight.DepartureTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ArrivalTime = flight.ArrivalTime.ToString("yyyy-MM-ddTHH:mm:ss")
                    },
                    passengers = passengers.Select(p => new
                    {
                        p.FirstName,
                        p.LastName,
                        p.SeatNumber,
                        p.SeatType
                    })
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching booking with PNR {PNR}", pnr);
                return Json(new { success = false, message = "Arama sırasında bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingRequest request)
        {
            try
            {
                if (request == null || request.FlightId <= 0 || !request.Passengers.Any())
                {
                    return BadRequest(new { message = "Geçersiz rezervasyon bilgileri." });
                }

                var flight = await _flightRepository.GetByIdAsync(request.FlightId);
                if (flight == null)
                {
                    return BadRequest(new { message = "Uçuş bulunamadı." });
                }

                if (flight.AvailableSeats < request.Passengers.Count)
                {
                    return BadRequest(new { message = "Yeterli koltuk bulunmuyor." });
                }

                // Get or create user
                User user = null;
                if (User.Identity.IsAuthenticated)
                {
                    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                    user = await _userRepository.GetByIdAsync(userId);
                }
                else if (!string.IsNullOrEmpty(request.GuestEmail))
                {
                    user = await _userRepository.GetByEmailAsync(request.GuestEmail);
                    if (user == null)
                    {
                        user = new User
                        {
                            Email = request.GuestEmail,
                            IsGuest = true,
                            IsAdmin = false,
                            CreatedAt = DateTime.UtcNow
                        };
                        await _userRepository.AddAsync(user);
                    }
                }

                if (user == null)
                {
                    return BadRequest(new { message = "Kullanıcı bilgileri bulunamadı." });
                }

                // Create booking
                var booking = new Booking
                {
                    BookingReference = GeneratePNR(),
                    UserId = user.Id,
                    FlightId = request.FlightId,
                    PassengerCount = request.Passengers.Count,
                    TotalPrice = flight.Price * request.Passengers.Count,
                    Status = "Confirmed",
                    BookingDate = DateTime.UtcNow,
                    IsPaid = false
                };

                await _bookingRepository.AddAsync(booking);

                // Create passengers
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
                        PhoneNumber = passengerDto.PhoneNumber,
                        SeatNumber = passengerDto.SeatNumber ?? "TBD",
                        SeatType = passengerDto.SeatType ?? "Economy"
                    };

                    await _passengerRepository.AddAsync(passenger);
                }

                // Update flight availability
                flight.AvailableSeats -= request.Passengers.Count;
                await _flightRepository.UpdateAsync(flight);

                // Send confirmation email
                try
                {
                    await _emailService.SendBookingConfirmationAsync(
                        user.Email,
                        booking.BookingReference,
                        flight,
                        request.Passengers.Select(p => $"{p.FirstName} {p.LastName}").ToList()
                    );
                }
                catch (Exception emailEx)
                {
                    _logger.LogWarning(emailEx, "Failed to send confirmation email for booking {BookingReference}", booking.BookingReference);
                }

                return Ok(new { bookingReference = booking.BookingReference });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { message = "Rezervasyon oluşturulurken bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CancelBooking(string pnr, string email)
        {
            try
            {
                var booking = await _bookingRepository.GetByPnrAndEmailAsync(pnr, email);
                
                if (booking == null)
                {
                    return Json(new { success = false, message = "Rezervasyon bulunamadı." });
                }

                if (booking.Status == "Cancelled")
                {
                    return Json(new { success = false, message = "Rezervasyon zaten iptal edilmiş." });
                }

                var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                
                // Check if cancellation is allowed (24 hours before departure)
                if (flight.DepartureTime <= DateTime.UtcNow.AddHours(24))
                {
                    return Json(new { success = false, message = "Uçuştan 24 saat öncesine kadar iptal edilebilir." });
                }

                // Cancel booking
                booking.Status = "Cancelled";
                await _bookingRepository.UpdateAsync(booking);

                // Release seats
                flight.AvailableSeats += booking.PassengerCount;
                await _flightRepository.UpdateAsync(flight);

                return Json(new { success = true, message = "Rezervasyon başarıyla iptal edildi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking with PNR {PNR}", pnr);
                return Json(new { success = false, message = "İptal işlemi sırasında bir hata oluştu." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ProcessPayment(string pnr, string email)
        {
            try
            {
                var booking = await _bookingRepository.GetByPnrAndEmailAsync(pnr, email);
                
                if (booking == null)
                {
                    return Json(new { success = false, message = "Rezervasyon bulunamadı." });
                }

                if (booking.IsPaid)
                {
                    return Json(new { success = false, message = "Bu rezervasyon için ödeme zaten yapılmış." });
                }

                if (booking.Status == "Cancelled")
                {
                    return Json(new { success = false, message = "İptal edilmiş rezervasyon için ödeme yapılamaz." });
                }

                // Simulate payment processing
                booking.IsPaid = true;
                booking.PaymentDate = DateTime.UtcNow;
                await _bookingRepository.UpdateAsync(booking);

                return Json(new { success = true, message = "Ödeme başarıyla tamamlandı." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for PNR {PNR}", pnr);
                return Json(new { success = false, message = "Ödeme işlemi sırasında bir hata oluştu." });
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
        public List<PassengerDto> Passengers { get; set; } = new();
        public string? GuestEmail { get; set; }
    }

    public class PassengerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string IdentityNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? SeatNumber { get; set; }
        public string? SeatType { get; set; }
    }
}