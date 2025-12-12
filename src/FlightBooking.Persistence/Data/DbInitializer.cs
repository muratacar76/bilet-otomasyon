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

        var flights = GenerateFlights();

        context.Flights.AddRange(flights);
        context.SaveChanges();
    }

    private static List<Flight> GenerateFlights()
    {
        var flights = new List<Flight>();
        var airlines = new[] { "Turkish Airlines", "Pegasus", "SunExpress", "AnadoluJet", "AtlasGlobal" };
        var cities = new[] { "İstanbul", "Ankara", "İzmir", "Antalya", "Trabzon", "Adana", "Gaziantep", "Kayseri", "Konya", "Bursa" };
        var random = new Random();
        var flightCounter = 1;

        // 14 gün boyunca her gün uçuşlar oluştur
        for (int day = 1; day <= 14; day++)
        {
            // Her gün 8-12 arası uçuş oluştur
            int dailyFlights = random.Next(8, 13);
            
            for (int i = 0; i < dailyFlights; i++)
            {
                var departureCity = cities[random.Next(cities.Length)];
                var arrivalCity = cities[random.Next(cities.Length)];
                
                // Aynı şehir olmasın
                while (arrivalCity == departureCity)
                {
                    arrivalCity = cities[random.Next(cities.Length)];
                }

                var airline = airlines[random.Next(airlines.Length)];
                var departureHour = random.Next(6, 23); // 06:00 - 22:59 arası
                var departureMinute = random.Next(0, 60);
                var flightDuration = random.Next(60, 180); // 1-3 saat arası

                var departureTime = DateTime.Now.AddDays(day).Date.AddHours(departureHour).AddMinutes(departureMinute);
                var arrivalTime = departureTime.AddMinutes(flightDuration);

                // Fiyat hesaplama (mesafeye göre)
                var basePrice = 400;
                var distanceMultiplier = random.Next(1, 4);
                var price = basePrice + (distanceMultiplier * 150) + random.Next(-50, 100);

                // Koltuk sayısı
                var totalSeats = random.Next(120, 201);
                var bookedSeats = random.Next(0, totalSeats / 3); // %0-33 arası dolu
                var availableSeats = totalSeats - bookedSeats;

                flights.Add(new Flight
                {
                    FlightNumber = GetFlightNumber(airline, flightCounter++),
                    Airline = airline,
                    DepartureCity = departureCity,
                    ArrivalCity = arrivalCity,
                    DepartureTime = departureTime,
                    ArrivalTime = arrivalTime,
                    Price = price,
                    TotalSeats = totalSeats,
                    AvailableSeats = availableSeats,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    SeatsPerRow = 6,
                    TotalRows = totalSeats / 6
                });
            }
        }

        return flights;
    }

    private static string GetFlightNumber(string airline, int counter)
    {
        var prefix = airline switch
        {
            "Turkish Airlines" => "TK",
            "Pegasus" => "PC",
            "SunExpress" => "XQ",
            "AnadoluJet" => "AJ",
            "AtlasGlobal" => "KK",
            _ => "XX"
        };
        
        return $"{prefix}{counter:D3}";
    }
}
