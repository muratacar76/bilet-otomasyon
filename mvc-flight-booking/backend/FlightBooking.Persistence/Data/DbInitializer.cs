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

        // Seed sample bookings for testing
        await SeedSampleBookings(context, flights);
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

        // First, add popular routes to ensure they exist
        var popularRoutes = new[]
        {
            ("İstanbul", "İzmir"),
            ("İzmir", "İstanbul"),
            ("İstanbul", "Ankara"),
            ("Ankara", "İstanbul"),
            ("İstanbul", "Antalya"),
            ("Antalya", "İstanbul"),
            ("İzmir", "Ankara"),
            ("Ankara", "İzmir")
        };

        // Add popular routes for next 7 days
        for (int day = 1; day <= 7; day++)
        {
            var baseDate = DateTime.Now.AddDays(day);
            
            foreach (var (departureCity, arrivalCity) in popularRoutes)
            {
                // Add 2-3 flights per popular route per day
                var flightsForRoute = random.Next(2, 4);
                
                for (int i = 0; i < flightsForRoute; i++)
                {
                    var airline = airlines[random.Next(airlines.Length)];
                    var departureHour = random.Next(6, 23);
                    var departureMinute = random.Next(0, 4) * 15;
                    var flightDuration = random.Next(60, 180); // 1-3 hours for domestic

                    var departureTime = baseDate.Date.AddHours(departureHour).AddMinutes(departureMinute);
                    var arrivalTime = departureTime.AddMinutes(flightDuration);

                    var airlineCode = airline switch
                    {
                        "Turkish Airlines" => "TK",
                        "Pegasus Airlines" => "PC",
                        "SunExpress" => "XQ",
                        "AnadoluJet" => "TK",
                        "AtlasGlobal" => "KK",
                        _ => "XX"
                    };

                    var basePrice = random.Next(300, 600); // Popular routes are a bit more expensive
                    var price = (decimal)(basePrice + random.Next(-50, 100));

                    var totalSeats = random.Next(150, 200);
                    var bookedSeats = random.Next(0, totalSeats / 3); // Less booking for testing
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
        }

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

    private static async Task SeedSampleBookings(ApplicationDbContext context, List<Flight> flights)
    {
        var random = new Random(42);
        
        // Create some test users
        var testUsers = new List<User>
        {
            new User
            {
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                Email = "ahmet@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                PhoneNumber = "05551234567",
                IdentityNumber = "12345678901",
                DateOfBirth = new DateTime(1985, 5, 15),
                Gender = "Erkek",
                IsGuest = false,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FirstName = "Fatma",
                LastName = "Kaya",
                Email = "fatma@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                PhoneNumber = "05559876543",
                IdentityNumber = "98765432109",
                DateOfBirth = new DateTime(1990, 8, 20),
                Gender = "Kadın",
                IsGuest = false,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FirstName = "Mehmet",
                LastName = "Demir",
                Email = "mehmet@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!"),
                PhoneNumber = "05555555555",
                IdentityNumber = "11111111111",
                DateOfBirth = new DateTime(1988, 3, 10),
                Gender = "Erkek",
                IsGuest = false,
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.Users.AddRangeAsync(testUsers);
        await context.SaveChangesAsync();

        // Create sample bookings
        var bookings = new List<Booking>();

        for (int i = 0; i < 10; i++)
        {
            var flight = flights[random.Next(flights.Count)];
            var user = testUsers[random.Next(testUsers.Count)];
            var passengerCount = random.Next(1, 4); // 1-3 passengers

            var booking = new Booking
            {
                BookingReference = GenerateTestPNR(i),
                UserId = user.Id,
                FlightId = flight.Id,
                PassengerCount = passengerCount,
                TotalPrice = flight.Price * passengerCount,
                Status = "Onaylandı",
                BookingDate = DateTime.UtcNow.AddDays(-random.Next(1, 30)),
                IsPaid = random.Next(0, 2) == 1, // 50% chance of being paid
                PaymentDate = null
            };

            if (booking.IsPaid)
            {
                booking.PaymentDate = booking.BookingDate.AddMinutes(random.Next(5, 60));
            }

            bookings.Add(booking);
        }

        await context.Bookings.AddRangeAsync(bookings);
        await context.SaveChangesAsync();

        // Now create passengers with correct booking IDs
        var allPassengers = new List<Passenger>();
        for (int i = 0; i < bookings.Count; i++)
        {
            var booking = bookings[i];
            var passengerCount = booking.PassengerCount;
            
            for (int p = 0; p < passengerCount; p++)
            {
                var passenger = new Passenger
                {
                    BookingId = booking.Id,
                    FirstName = GenerateRandomFirstName(random),
                    LastName = GenerateRandomLastName(random),
                    IdentityNumber = GenerateRandomTC(random),
                    DateOfBirth = DateTime.Now.AddYears(-random.Next(18, 65)),
                    Gender = random.Next(0, 2) == 0 ? "Erkek" : "Kadın",
                    PhoneNumber = $"0555{random.Next(1000000, 9999999)}",
                    SeatNumber = $"{random.Next(1, 30)}{(char)('A' + random.Next(0, 6))}",
                    SeatType = "Economy"
                };

                allPassengers.Add(passenger);
            }
        }

        await context.Passengers.AddRangeAsync(allPassengers);
        await context.SaveChangesAsync();
    }

    private static string GenerateTestPNR(int index)
    {
        var pnrs = new[] { "ABC123", "DEF456", "GHI789", "JKL012", "MNO345", "PQR678", "STU901", "VWX234", "YZA567", "BCD890" };
        return pnrs[index % pnrs.Length];
    }

    private static string GenerateRandomFirstName(Random random)
    {
        var names = new[] { "Ahmet", "Mehmet", "Mustafa", "Ali", "Hasan", "Hüseyin", "İbrahim", "İsmail", "Ömer", "Osman",
                           "Fatma", "Ayşe", "Emine", "Hatice", "Zeynep", "Elif", "Meryem", "Büşra", "Seda", "Özlem" };
        return names[random.Next(names.Length)];
    }

    private static string GenerateRandomLastName(Random random)
    {
        var surnames = new[] { "Yılmaz", "Kaya", "Demir", "Şahin", "Çelik", "Yıldız", "Yıldırım", "Öztürk", "Aydin", "Özkan",
                              "Kaplan", "Çetin", "Kara", "Can", "Korkmaz", "Özdemir", "Arslan", "Doğan", "Kilic", "Aslan" };
        return surnames[random.Next(surnames.Length)];
    }

    private static string GenerateRandomTC(Random random)
    {
        // Generate a valid-looking TC number (not real validation)
        var tc = "";
        for (int i = 0; i < 11; i++)
        {
            tc += random.Next(0, 10).ToString();
        }
        return tc;
    }
}