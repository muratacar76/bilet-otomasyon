using FlightBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Data;

/// <summary>
/// Database initializer for seeding initial data
/// </summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return; // Database has been seeded
        }

        // Seed admin user
        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@bulutbilet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            PhoneNumber = "05551234567",
            IdentityNumber = "12345678901",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "Erkek",
            IsGuest = false,
            IsAdmin = true,
            CreatedAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(adminUser);

        // Seed comprehensive flight data
        var flights = GenerateFlights();

        await context.Flights.AddRangeAsync(flights);
        await context.SaveChangesAsync();

        // Generate seats for each flight
        foreach (var flight in flights)
        {
            await GenerateSeatsForFlight(context, flight);
        }

        await context.SaveChangesAsync();
    }

    private static async Task GenerateSeatsForFlight(ApplicationDbContext context, Flight flight)
    {
        var seats = new List<Seat>();
        var seatLetters = new[] { "A", "B", "C", "D", "E", "F" };

        for (int row = 1; row <= flight.TotalRows; row++)
        {
            for (int col = 0; col < Math.Min(flight.SeatsPerRow, seatLetters.Length); col++)
            {
                var seatNumber = $"{row}{seatLetters[col]}";
                var seatType = (col == 0 || col == flight.SeatsPerRow - 1) ? "Window" :
                               (col == 1 || col == flight.SeatsPerRow - 2) ? "Aisle" : "Middle";

                // Mark exit rows (1, 10, 20, 30)
                var isExitRow = row == 1 || row == 10 || row == 20 || row == 30;

                seats.Add(new Seat
                {
                    FlightId = flight.Id,
                    SeatNumber = seatNumber,
                    SeatType = seatType,
                    IsOccupied = false,
                    Row = row,
                    Column = seatLetters[col],
                    IsExitRow = isExitRow,
                    IsBlocked = false
                });
            }
        }

        await context.Seats.AddRangeAsync(seats);
    }

    private static List<Flight> GenerateFlights()
    {
        var flights = new List<Flight>();
        var airlines = new[] { "Turkish Airlines", "Pegasus Airlines", "SunExpress", "AnadoluJet", "AtlasGlobal" };
        var cities = new[] { "İstanbul", "Ankara", "İzmir", "Antalya", "Adana", "Trabzon", "Erzurum", "Diyarbakır", "Gaziantep", "Kayseri", "Konya", "Samsun", "Denizli", "Bodrum", "Dalaman", "Nevşehir" };
        
        var random = new Random(42); // Fixed seed for consistent data
        var flightCounter = 1;

        // Generate flights for next 30 days
        for (int day = 1; day <= 30; day++)
        {
            var baseDate = DateTime.Now.AddDays(day);
            
            // Generate 4-6 flights per day
            var flightsPerDay = random.Next(4, 7);
            
            for (int i = 0; i < flightsPerDay; i++)
            {
                var airline = airlines[random.Next(airlines.Length)];
                var departureCity = cities[random.Next(cities.Length)];
                var arrivalCity = cities[random.Next(cities.Length)];
                
                // Ensure different cities
                while (arrivalCity == departureCity)
                {
                    arrivalCity = cities[random.Next(cities.Length)];
                }

                var departureHour = random.Next(6, 23); // 6 AM to 11 PM
                var departureMinute = random.Next(0, 4) * 15; // 0, 15, 30, 45
                var flightDuration = random.Next(60, 300); // 1-5 hours

                var departureTime = baseDate.Date.AddHours(departureHour).AddMinutes(departureMinute);
                var arrivalTime = departureTime.AddMinutes(flightDuration);

                var airlineCode = airline switch
                {
                    "Turkish Airlines" => "TK",
                    "Pegasus Airlines" => "PC",
                    "SunExpress" => "XQ",
                    "AnadoluJet" => "TK", // AnadoluJet uses TK codes
                    "AtlasGlobal" => "KK",
                    _ => "XX"
                };

                var basePrice = random.Next(200, 800);
                var price = (decimal)(basePrice + random.Next(-50, 100));

                var totalSeats = random.Next(150, 200);
                var bookedSeats = random.Next(0, totalSeats / 2); // Random booking level
                var availableSeats = totalSeats - bookedSeats;

                flights.Add(new Flight
                {
                    FlightNumber = $"{airlineCode}{flightCounter:D3}",
                    Airline = airline,
                    DepartureCity = departureCity,
                    ArrivalCity = arrivalCity,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                    Price = price,
                    TotalSeats = totalSeats,
                    AvailableSeats = availableSeats,
                    Status = "Active",
                    SeatsPerRow = 6,
                    TotalRows = totalSeats / 6,
                    CreatedAt = DateTime.UtcNow
                });

                flightCounter++;
            }
        }

        return flights;
    }
}