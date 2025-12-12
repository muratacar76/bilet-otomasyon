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
        public IActionResult Query(string? pnr = null)
        {
            ViewData["Title"] = "PNR Sorgula - BULUTBİLET.COM";
            ViewBag.PNR = pnr;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Manage(string? pnr = null, string? email = null)
        {
            try
            {
                _logger.LogInformation("Manage action called with PNR: {PNR}, Email: {Email}", pnr, email);
                ViewData["Title"] = "Rezervasyon Yönetimi - BULUTBİLET.COM";
                
                // If PNR and email are provided, search for the booking
                if (!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(email))
                {
                    _logger.LogInformation("Searching for booking with PNR: {PNR} and email: {Email}", pnr, email);
                    
                    // Trim and normalize inputs
                    pnr = pnr.Trim().ToUpper();
                    email = email.Trim().ToLower();
                    
                    var booking = await _bookingRepository.GetByPnrAndEmailAsync(pnr, email);
                    
                    if (booking == null)
                    {
                        _logger.LogWarning("Booking not found for PNR: {PNR} and email: {Email}", pnr, email);
                        
                        // Check if PNR exists with different email
                        var bookingByPnr = await _bookingRepository.GetByReferenceAsync(pnr);
                        if (bookingByPnr != null)
                        {
                            TempData["Error"] = $"PNR '{pnr}' bulundu ancak e-posta adresi eşleşmiyor. Lütfen rezervasyon sırasında kullandığınız e-posta adresini girin.";
                        }
                        else
                        {
                            TempData["Error"] = $"PNR '{pnr}' bulunamadı. Lütfen PNR numaranızı kontrol edin.";
                        }
                        
                        return RedirectToAction("Query", new { pnr = pnr, email = email });
                    }

                    _logger.LogInformation("Booking found: {BookingId}", booking.Id);

                    var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                    if (flight == null)
                    {
                        _logger.LogError("Flight not found for booking {BookingId}, FlightId: {FlightId}", booking.Id, booking.FlightId);
                        TempData["Error"] = "Rezervasyona ait uçuş bilgileri bulunamadı.";
                        return RedirectToAction("Query");
                    }

                    var passengers = await _passengerRepository.GetByBookingIdAsync(booking.Id);
                    var user = booking.User ?? await _userRepository.GetByIdAsync(booking.UserId);

                    // Add CanBeCancelled property
                    var canBeCancelled = booking.Status == "Onaylandı" && 
                                       flight.DepartureTime > DateTime.UtcNow.AddHours(24);

                    var viewModel = new
                    {
                        Booking = new
                        {
                            booking.Id,
                            booking.BookingReference,
                            booking.Status,
                            booking.TotalPrice,
                            booking.BookingDate,
                            booking.IsPaid,
                            booking.PaymentDate,
                            booking.PassengerCount,
                            CanBeCancelled = canBeCancelled
                        },
                        Flight = flight,
                        Passengers = passengers.ToList(),
                        User = user
                    };

                    _logger.LogInformation("Booking details loaded successfully for PNR: {PNR}", pnr);
                    return View(viewModel);
                }
                
                // If no parameters, redirect to query page
                _logger.LogInformation("No parameters provided, redirecting to query page");
                return RedirectToAction("Query");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Manage action with PNR {PNR} and email {Email}", pnr, email);
                TempData["Error"] = "Rezervasyon aranırken bir sistem hatası oluştu: " + ex.Message;
                return RedirectToAction("Query");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> MyBookings()
        {
            try
            {
                _logger.LogInformation("MyBookings action called");
                
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("User ID claim: {UserIdClaim}", userIdClaim);
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId) || userId <= 0)
                {
                    _logger.LogWarning("Invalid or missing user ID claim: {UserIdClaim}", userIdClaim);
                    TempData["Error"] = "Kullanıcı bilgileri alınamadı. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Auth");
                }

                _logger.LogInformation("Loading bookings for user ID: {UserId}", userId);
                
                // Verify user exists
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for ID: {UserId}", userId);
                    TempData["Error"] = "Kullanıcı bulunamadı. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Auth");
                }

                _logger.LogInformation("User found: {UserEmail}", user.Email);

                var bookings = await _bookingRepository.GetByUserIdAsync(userId);
                _logger.LogInformation("Found {BookingCount} bookings for user {UserId}", bookings.Count(), userId);
                
                // Debug: Check if there are any bookings in the database at all
                var allBookings = await _bookingRepository.GetAllAsync();
                _logger.LogInformation("Total bookings in database: {TotalBookings}", allBookings.Count());
                

                
                var bookingViewModels = new List<object>();
                
                // Eğer rezervasyon yoksa boş liste döndür
                ViewData["Title"] = "Rezervasyonlarım - BULUTBİLET.COM";

                
                if (!bookings.Any())
                {
                    _logger.LogInformation("No bookings found for user {UserId}", userId);
                    return View(bookingViewModels);
                }

                foreach (var booking in bookings)
                {
                    try
                    {
                        _logger.LogInformation("Processing booking {BookingId}", booking.Id);
                        _logger.LogInformation("Booking details: Id={Id}, PNR='{PNR}', Status='{Status}', UserId={UserId}", 
                            booking.Id, booking.BookingReference, booking.Status, booking.UserId);
                        
                        var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                        if (flight == null)
                        {
                            _logger.LogWarning("Flight not found for booking {BookingId}, FlightId: {FlightId}", booking.Id, booking.FlightId);
                            continue;
                        }

                        var passengers = await _passengerRepository.GetByBookingIdAsync(booking.Id);
                        _logger.LogInformation("Found {PassengerCount} passengers for booking {BookingId}", passengers.Count(), booking.Id);
                        
                        _logger.LogInformation("Booking {BookingId} has PNR: '{PNR}'", booking.Id, booking.BookingReference);
                        
                        var viewModel = new
                        {
                            Id = booking.Id,
                            BookingReference = booking.BookingReference ?? "N/A",
                            FlightNumber = flight.FlightNumber ?? "",
                            Airline = flight.Airline ?? "",
                            DepartureCity = flight.DepartureCity ?? "",
                            ArrivalCity = flight.ArrivalCity ?? "",
                            DepartureTime = flight.DepartureTime,
                            ArrivalTime = flight.ArrivalTime,
                            TotalPrice = booking.TotalPrice,
                            Status = booking.Status ?? "Unknown",
                            IsPaid = booking.IsPaid,
                            BookingDate = booking.BookingDate,
                            CanBeCancelled = booking.Status == "Onaylandı" && flight.DepartureTime > DateTime.UtcNow.AddHours(24),
                            Passengers = passengers.Select(p => new {
                                p.FirstName,
                                p.LastName,
                                p.SeatNumber,
                                p.SeatType
                            }).ToList()
                        };
                        
                        bookingViewModels.Add(viewModel);
                        _logger.LogInformation("Successfully added booking {BookingId} to view models", booking.Id);
                    }
                    catch (Exception bookingEx)
                    {
                        _logger.LogError(bookingEx, "Error loading booking {BookingId}: {ErrorMessage}", booking.Id, bookingEx.Message);
                        // Continue with other bookings
                    }
                }

                _logger.LogInformation("Successfully loaded {Count} bookings for user {UserId}", bookingViewModels.Count, userId);
                return View(bookingViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user bookings");
                TempData["Error"] = "Rezervasyonlar yüklenirken bir sistem hatası oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SearchByEmail(string email, string identityNumber)
        {
            try
            {
                _logger.LogInformation("SearchByEmail action called with email: {Email}, identityNumber: {IdentityNumber}", email, identityNumber);
                ViewData["Title"] = "E-posta ile Rezervasyon Sorgulama - BULUTBİLET.COM";
                
                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogWarning("Email parameter is null or empty");
                    TempData["Error"] = "E-posta adresi gereklidir.";
                    return RedirectToAction("Query");
                }

                if (string.IsNullOrEmpty(identityNumber))
                {
                    _logger.LogWarning("Identity number parameter is null or empty");
                    TempData["Error"] = "TC kimlik numarası gereklidir.";
                    return RedirectToAction("Query");
                }

                if (identityNumber.Length != 11 || !identityNumber.All(char.IsDigit))
                {
                    _logger.LogWarning("Invalid identity number format: {IdentityNumber}", identityNumber);
                    TempData["Error"] = "TC kimlik numarası 11 haneli olmalı ve sadece rakam içermelidir.";
                    return RedirectToAction("Query");
                }

                _logger.LogInformation("Searching bookings for email: {Email} and identity number: {IdentityNumber}", email, identityNumber);
                
                var bookings = await _bookingRepository.GetBookingsByEmailAsync(email);
                _logger.LogInformation("Found {Count} bookings for email: {Email}", bookings.Count(), email);
                
                var bookingViewModels = new List<object>();
                
                if (!bookings.Any())
                {
                    _logger.LogWarning("No bookings found for email: {Email}", email);
                    ViewBag.SearchEmail = email;
                    ViewBag.SearchIdentityNumber = identityNumber;
                    return View("EmailBookings", bookingViewModels);
                }

                foreach (var booking in bookings)
                {
                    try
                    {
                        _logger.LogInformation("Processing booking {BookingId} for email search", booking.Id);
                        
                        var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                        if (flight == null)
                        {
                            _logger.LogWarning("Flight not found for booking {BookingId}, FlightId: {FlightId}", booking.Id, booking.FlightId);
                            continue;
                        }

                        var passengers = await _passengerRepository.GetByBookingIdAsync(booking.Id);
                        
                        // TC kimlik kontrolü - zorunlu
                        var hasMatchingPassenger = passengers.Any(p => p.IdentityNumber == identityNumber);
                        if (!hasMatchingPassenger)
                        {
                            _logger.LogInformation("Booking {BookingId} filtered out - no matching identity number", booking.Id);
                            continue;
                        }
                        
                        var user = booking.User ?? await _userRepository.GetByIdAsync(booking.UserId);
                        
                        bookingViewModels.Add(new
                        {
                            Booking = new
                            {
                                booking.Id,
                                booking.BookingReference,
                                booking.Status,
                                booking.TotalPrice,
                                booking.BookingDate,
                                booking.IsPaid,
                                booking.PaymentDate,
                                booking.PassengerCount
                            },
                            Flight = new
                            {
                                flight.Id,
                                flight.FlightNumber,
                                flight.Airline,
                                flight.DepartureCity,
                                flight.ArrivalCity,
                                flight.DepartureTime,
                                flight.ArrivalTime,
                                flight.Price
                            },
                            Passengers = passengers.Select(p => new
                            {
                                p.Id,
                                p.FirstName,
                                p.LastName,
                                p.IdentityNumber,
                                p.PhoneNumber,
                                p.DateOfBirth,
                                p.Gender,
                                p.SeatNumber,
                                p.SeatType
                            }).ToList(),
                            User = new
                            {
                                user.Id,
                                user.Email,
                                user.FirstName,
                                user.LastName,
                                user.PhoneNumber
                            }
                        });
                    }
                    catch (Exception bookingEx)
                    {
                        _logger.LogError(bookingEx, "Error processing booking {BookingId}", booking.Id);
                        // Continue with other bookings
                    }
                }

                _logger.LogInformation("Successfully processed {Count} bookings for email: {Email} and identity: {IdentityNumber}", bookingViewModels.Count, email, identityNumber);
                ViewBag.SearchEmail = email;
                ViewBag.SearchIdentityNumber = identityNumber;
                return View("EmailBookings", bookingViewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching bookings by email {Email}", email);
                TempData["Error"] = "Rezervasyon aranırken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Query");
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
                    Status = "Onaylandı",
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

        [HttpGet]
        public async Task<IActionResult> Payment(string pnr, string? email = null)
        {
            try
            {
                ViewData["Title"] = "Ödeme - BULUTBİLET.COM";
                
                // Eğer kullanıcı giriş yapmışsa email'i al
                if (string.IsNullOrEmpty(email) && User.Identity.IsAuthenticated)
                {
                    email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                }
                
                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "E-posta adresi gereklidir.";
                    return RedirectToAction("Query");
                }

                var booking = await _bookingRepository.GetByPnrAndEmailAsync(pnr, email);
                if (booking == null)
                {
                    TempData["Error"] = "Rezervasyon bulunamadı.";
                    return RedirectToAction("Query");
                }

                if (booking.IsPaid)
                {
                    TempData["Error"] = "Bu rezervasyon için ödeme zaten yapılmış.";
                    return RedirectToAction("Manage", new { pnr = pnr, email = email });
                }

                var flight = await _flightRepository.GetByIdAsync(booking.FlightId);
                if (flight == null)
                {
                    TempData["Error"] = "Uçuş bilgileri bulunamadı.";
                    return RedirectToAction("Query");
                }

                var viewModel = new
                {
                    Booking = new
                    {
                        booking.Id,
                        booking.BookingReference,
                        booking.Status,
                        booking.TotalPrice,
                        booking.BookingDate,
                        booking.IsPaid
                    },
                    Flight = flight
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading payment page for PNR {PNR}", pnr);
                TempData["Error"] = "Ödeme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Query");
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



        [HttpPost]
        [Route("/api/bookings/create-test")]
        [Authorize]
        public async Task<IActionResult> CreateTestBooking()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest("Kullanıcı ID'si alınamadı");
                }

                // Get first available flight
                var flights = await _flightRepository.GetAllAsync();
                var flight = flights.FirstOrDefault();
                if (flight == null)
                {
                    return BadRequest("Uçuş bulunamadı");
                }

                // Create test booking
                var booking = new Booking
                {
                    BookingReference = GeneratePNR(),
                    UserId = userId,
                    FlightId = flight.Id,
                    PassengerCount = 1,
                    TotalPrice = flight.Price,
                    Status = "Onaylandı",
                    BookingDate = DateTime.UtcNow,
                    IsPaid = true,
                    PaymentDate = DateTime.UtcNow
                };

                await _bookingRepository.AddAsync(booking);

                // Create test passenger
                var passenger = new Passenger
                {
                    BookingId = booking.Id,
                    FirstName = "Test",
                    LastName = "Kullanıcı",
                    IdentityNumber = "12345678901",
                    DateOfBirth = DateTime.Now.AddYears(-30),
                    Gender = "Erkek",
                    PhoneNumber = "05551234567",
                    SeatNumber = "1A",
                    SeatType = "Economy"
                };

                await _passengerRepository.AddAsync(passenger);

                return Ok(new { pnr = booking.BookingReference, message = "Test rezervasyonu oluşturuldu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test booking");
                return StatusCode(500, "Test rezervasyonu oluşturulamadı: " + ex.Message);
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