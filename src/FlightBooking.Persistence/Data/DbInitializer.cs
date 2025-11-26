using FlightBooking.Core.Entities;

namespace FlightBooking.Persistence.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Flights.Any())
        {
            return; // DB has been seeded
        }

        var flights = new Flight[]
        {
            new Flight
            {
                FlightNumber = "TK101",
                Airline = "Turkish Airlines",
                DepartureCity = "İstanbul",
                ArrivalCity = "Ankara",
                DepartureTime = DateTime.Now.AddDays(2).AddHours(9),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(10).AddMinutes(30),
                Price = 850,
                TotalSeats = 180,
                AvailableSeats = 180,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "PC202",
                Airline = "Pegasus",
                DepartureCity = "İstanbul",
                ArrivalCity = "İzmir",
                DepartureTime = DateTime.Now.AddDays(1).AddHours(14),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(15).AddMinutes(20),
                Price = 650,
                TotalSeats = 150,
                AvailableSeats = 150,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "TK303",
                Airline = "Turkish Airlines",
                DepartureCity = "Ankara",
                ArrivalCity = "Antalya",
                DepartureTime = DateTime.Now.AddDays(3).AddHours(11),
                ArrivalTime = DateTime.Now.AddDays(3).AddHours(12).AddMinutes(45),
                Price = 920,
                TotalSeats = 200,
                AvailableSeats = 200,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "SJ404",
                Airline = "SunExpress",
                DepartureCity = "İzmir",
                ArrivalCity = "Antalya",
                DepartureTime = DateTime.Now.AddDays(2).AddHours(16),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(17).AddMinutes(10),
                Price = 580,
                TotalSeats = 120,
                AvailableSeats = 120,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "PC505",
                Airline = "Pegasus",
                DepartureCity = "İstanbul",
                ArrivalCity = "Trabzon",
                DepartureTime = DateTime.Now.AddDays(4).AddHours(8),
                ArrivalTime = DateTime.Now.AddDays(4).AddHours(10).AddMinutes(15),
                Price = 780,
                TotalSeats = 140,
                AvailableSeats = 140,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "TK606",
                Airline = "Turkish Airlines",
                DepartureCity = "Ankara",
                ArrivalCity = "İstanbul",
                DepartureTime = DateTime.Now.AddDays(1).AddHours(18),
                ArrivalTime = DateTime.Now.AddDays(1).AddHours(19).AddMinutes(30),
                Price = 890,
                TotalSeats = 180,
                AvailableSeats = 180,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "AJ707",
                Airline = "AnadoluJet",
                DepartureCity = "İzmir",
                ArrivalCity = "İstanbul",
                DepartureTime = DateTime.Now.AddDays(2).AddHours(12),
                ArrivalTime = DateTime.Now.AddDays(2).AddHours(13).AddMinutes(20),
                Price = 720,
                TotalSeats = 160,
                AvailableSeats = 160,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            },
            new Flight
            {
                FlightNumber = "TK808",
                Airline = "Turkish Airlines",
                DepartureCity = "Antalya",
                ArrivalCity = "İstanbul",
                DepartureTime = DateTime.Now.AddDays(3).AddHours(20),
                ArrivalTime = DateTime.Now.AddDays(3).AddHours(21).AddMinutes(40),
                Price = 950,
                TotalSeats = 200,
                AvailableSeats = 200,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Flights.AddRange(flights);
        context.SaveChanges();
    }
}
