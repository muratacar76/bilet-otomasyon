using Microsoft.AspNetCore.Mvc;
using FlightBooking.Core.Interfaces;
using FlightBooking.Web.Models.ViewModels;
using FlightBooking.Core.Entities;

namespace FlightBooking.Web.Controllers
{
    public class FlightController : Controller
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ILogger<FlightController> _logger;

        public FlightController(IFlightRepository flightRepository, ILogger<FlightController> logger)
        {
            _flightRepository = flightRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? showAll = null)
        {
            ViewData["Title"] = "Uçuş Ara - Bulut Bilet.com";
            
            var model = new FlightSearchViewModel
            {
                DepartureDate = DateTime.Today,
                PassengerCount = 1,
                DepartureCity = "", // Clear required fields for showAll
                ArrivalCity = ""    // Clear required fields for showAll
            };

            // If showAll parameter is present, load all flights immediately
            if (!string.IsNullOrEmpty(showAll) && showAll.ToLower() == "true")
            {
                try
                {
                    _logger.LogInformation("Loading all flights for showAll=true");
                    var allFlights = await _flightRepository.GetAllAsync();
                    _logger.LogInformation("Retrieved {Count} flights from repository", allFlights.Count());
                    
                    var activeFlights = allFlights.Where(f => f.Status == "Active").ToList();
                    _logger.LogInformation("Found {Count} active flights", activeFlights.Count);
                    
                    model.Flights = activeFlights;
                    model.TotalResults = activeFlights.Count;
                    model.HasSearched = true;
                    model.SearchMessage = $"Tüm mevcut uçuşlar ({activeFlights.Count})";
                    ViewBag.ShowAllFlights = true;
                    
                    _logger.LogInformation("Successfully loaded all flights for display");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error loading all flights");
                    model.SearchMessage = "Uçuşlar yüklenirken bir hata oluştu: " + ex.Message;
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Search(FlightSearchViewModel model)
        {
            ViewData["Title"] = "Uçuş Ara - Bulut Bilet.com";
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Validate search criteria
                if (model.DepartureCity == model.ArrivalCity)
                {
                    ModelState.AddModelError("", "Kalkış ve varış şehri aynı olamaz.");
                    return View(model);
                }

                if (model.DepartureDate < DateTime.Today)
                {
                    ModelState.AddModelError("DepartureDate", "Geçmiş tarih seçilemez.");
                    return View(model);
                }

                // Search flights
                var flights = await SearchFlightsAsync(model);
                
                model.Flights = flights;
                model.TotalResults = flights.Count;
                model.HasSearched = true;

                if (flights.Count == 0)
                {
                    model.SearchMessage = "Aradığınız kriterlerde uçuş bulunamadı. Lütfen farklı tarih veya şehir deneyin.";
                }
                else
                {
                    model.SearchMessage = $"{flights.Count} uçuş bulundu.";
                }

                ViewBag.SearchPerformed = true;
                _logger.LogInformation("Flight search performed: {DepartureCity} to {ArrivalCity} on {DepartureDate}, found {Count} flights", 
                    model.DepartureCity, model.ArrivalCity, model.DepartureDate, flights.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights");
                ModelState.AddModelError("", "Uçuş arama sırasında bir hata oluştu. Lütfen tekrar deneyin.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var flight = await _flightRepository.GetByIdAsync(id);
                
                if (flight == null)
                {
                    return NotFound("Uçuş bulunamadı.");
                }

                ViewData["Title"] = $"Uçuş Detayları - {flight.FlightNumber} - Bulut Bilet.com";
                return View(flight);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flight details for ID {FlightId}", id);
                return BadRequest("Uçuş detayları alınırken bir hata oluştu.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetFlightDetails(int id)
        {
            try
            {
                var flight = await _flightRepository.GetByIdAsync(id);
                
                if (flight == null)
                {
                    return NotFound();
                }

                var flightDetails = new
                {
                    flight.Id,
                    flight.FlightNumber,
                    flight.Airline,
                    flight.DepartureCity,
                    flight.ArrivalCity,
                    DepartureTime = flight.DepartureTime.ToString("dd.MM.yyyy HH:mm"),
                    ArrivalTime = flight.ArrivalTime.ToString("dd.MM.yyyy HH:mm"),
                    Duration = CalculateFlightDuration(flight.DepartureTime, flight.ArrivalTime),
                    flight.Price,
                    flight.AvailableSeats,
                    flight.TotalSeats,
                    AircraftType = "Boeing 737-800", // This could be added to the Flight entity
                    BaggageAllowance = "20 kg",
                    HandBaggage = "8 kg",
                    MealService = "Var",
                    CancellationPolicy = "Ücretli iptal",
                    ChangePolicy = "Ücretli değişiklik",
                    RefundPolicy = "Kısmi iade"
                };

                return Json(flightDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flight details for ID {FlightId}", id);
                return StatusCode(500, "Uçuş detayları alınırken bir hata oluştu.");
            }
        }

        private async Task<List<Flight>> SearchFlightsAsync(FlightSearchViewModel searchModel)
        {
            try
            {
                // Get all flights first (in a real application, this would be more optimized)
                var allFlights = await _flightRepository.GetAllAsync();
                
                // Apply filters
                var filteredFlights = allFlights.AsQueryable();

                if (!string.IsNullOrEmpty(searchModel.DepartureCity))
                {
                    filteredFlights = filteredFlights.Where(f => 
                        f.DepartureCity.Equals(searchModel.DepartureCity, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(searchModel.ArrivalCity))
                {
                    filteredFlights = filteredFlights.Where(f => 
                        f.ArrivalCity.Equals(searchModel.ArrivalCity, StringComparison.OrdinalIgnoreCase));
                }

                if (searchModel.DepartureDate != default(DateTime))
                {
                    filteredFlights = filteredFlights.Where(f => 
                        f.DepartureTime.Date == searchModel.DepartureDate.Date);
                }

                filteredFlights = filteredFlights.Where(f => 
                    f.AvailableSeats >= searchModel.PassengerCount &&
                    f.Status == "Active");

                // Apply optional filters
                if (!string.IsNullOrEmpty(searchModel.Airline))
                {
                    filteredFlights = filteredFlights.Where(f => 
                        f.Airline.Equals(searchModel.Airline, StringComparison.OrdinalIgnoreCase));
                }

                if (searchModel.MaxPrice.HasValue)
                {
                    filteredFlights = filteredFlights.Where(f => f.Price <= searchModel.MaxPrice.Value);
                }

                var result = filteredFlights.ToList();

                // Apply sorting
                result = searchModel.SortBy?.ToLower() switch
                {
                    "departuretime" => result.OrderBy(f => f.DepartureTime).ToList(),
                    "duration" => result.OrderBy(f => f.ArrivalTime - f.DepartureTime).ToList(),
                    "price" or _ => result.OrderBy(f => f.Price).ToList()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchFlightsAsync");
                return new List<Flight>();
            }
        }



        private static string CalculateFlightDuration(DateTime departure, DateTime arrival)
        {
            var duration = arrival - departure;
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;
            return $"{hours}s {minutes}dk";
        }

        // API endpoints for AJAX calls
        [HttpPost]
        public async Task<IActionResult> SearchApi([FromBody] FlightSearchViewModel searchModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var flights = await SearchFlightsAsync(searchModel);
                
                return Json(new
                {
                    success = true,
                    data = flights.Select(f => new
                    {
                        f.Id,
                        f.FlightNumber,
                        f.Airline,
                        f.DepartureCity,
                        f.ArrivalCity,
                        DepartureTime = f.DepartureTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        ArrivalTime = f.ArrivalTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        f.Price,
                        f.AvailableSeats
                    }),
                    count = flights.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchApi");
                return Json(new
                {
                    success = false,
                    message = "Uçuş arama sırasında bir hata oluştu."
                });
            }
        }

        /// <summary>
        /// Get flights for AJAX requests
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetFlights([FromQuery] string? departureCity = null, 
                                                   [FromQuery] string? arrivalCity = null, 
                                                   [FromQuery] DateTime? departureDate = null)
        {
            try
            {
                _logger.LogInformation("GetFlights called with: departureCity={DepartureCity}, arrivalCity={ArrivalCity}, departureDate={DepartureDate}", 
                    departureCity, arrivalCity, departureDate);

                var flights = await _flightRepository.GetAllAsync();
                
                // Filter only active flights first
                var activeFlights = flights.Where(f => f.Status == "Active");
                
                // Apply search filters
                if (!string.IsNullOrEmpty(departureCity))
                {
                    activeFlights = activeFlights.Where(f => f.DepartureCity.Equals(departureCity, StringComparison.OrdinalIgnoreCase));
                }
                
                if (!string.IsNullOrEmpty(arrivalCity))
                {
                    activeFlights = activeFlights.Where(f => f.ArrivalCity.Equals(arrivalCity, StringComparison.OrdinalIgnoreCase));
                }
                
                if (departureDate.HasValue)
                {
                    var date = departureDate.Value.Date;
                    activeFlights = activeFlights.Where(f => f.DepartureTime.Date == date);
                }

                var result = activeFlights.OrderBy(f => f.DepartureTime).Select(f => new
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

                _logger.LogInformation("GetFlights returning {Count} flights", result.Count);
                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting flights");
                return Json(new List<object>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPopularDestinations()
        {
            try
            {
                // This could be cached or stored in database
                var popularDestinations = new[]
                {
                    new { City = "İstanbul", Code = "IST", Icon = "fas fa-city", Description = "Türkiye'nin kalbi" },
                    new { City = "Antalya", Code = "AYT", Icon = "fas fa-umbrella-beach", Description = "Türk Rivierası" },
                    new { City = "Ankara", Code = "ESB", Icon = "fas fa-landmark", Description = "Başkent" },
                    new { City = "İzmir", Code = "ADB", Icon = "fas fa-water", Description = "Ege'nin İncisi" },
                    new { City = "Trabzon", Code = "TZX", Icon = "fas fa-mountain", Description = "Karadeniz'in İncisi" },
                    new { City = "Gaziantep", Code = "GZT", Icon = "fas fa-utensils", Description = "Gastronomi Şehri" }
                };

                return Json(popularDestinations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular destinations");
                return StatusCode(500, "Popüler destinasyonlar alınırken bir hata oluştu.");
            }
        }
    }
}