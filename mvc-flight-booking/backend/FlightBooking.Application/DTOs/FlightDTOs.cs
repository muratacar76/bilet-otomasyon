namespace FlightBooking.Application.DTOs;

/// <summary>
/// Flight search request DTO
/// </summary>
public class FlightSearchRequest
{
    public string? DepartureCity { get; set; }
    public string? ArrivalCity { get; set; }
    public DateTime? DepartureDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int PassengerCount { get; set; } = 1;
}

/// <summary>
/// Flight DTO for responses
/// </summary>
public class FlightDto
{
    public int Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SeatsPerRow { get; set; }
    public int TotalRows { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Computed properties
    public string Route { get; set; } = string.Empty;
    public TimeSpan FlightDuration { get; set; }
    public bool IsAvailable { get; set; }
    public int BookedSeats { get; set; }
}

/// <summary>
/// Flight creation/update request DTO
/// </summary>
public class FlightRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
    public string Status { get; set; } = "Active";
    public int SeatsPerRow { get; set; } = 6;
    public int TotalRows { get; set; } = 30;
}

/// <summary>
/// Seat DTO for responses
/// </summary>
public class SeatDto
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public int Row { get; set; }
    public string Column { get; set; } = string.Empty;
    public bool IsExitRow { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsAvailable { get; set; }
    public string SeatTypeDisplay { get; set; } = string.Empty;
}

/// <summary>
/// Seat row DTO for seat map
/// </summary>
public class SeatRowDto
{
    public int Row { get; set; }
    public List<SeatDto> Seats { get; set; } = new();
}

/// <summary>
/// Seat map response DTO
/// </summary>
public class SeatMapResponse
{
    public int FlightId { get; set; }
    public int SeatsPerRow { get; set; }
    public int TotalRows { get; set; }
    public List<string> OccupiedSeats { get; set; } = new();
    public List<SeatRowDto> SeatLayout { get; set; } = new();
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
}

/// <summary>
/// Airport information DTO
/// </summary>
public class AirportDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

/// <summary>
/// Create flight DTO
/// </summary>
public class CreateFlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
}

/// <summary>
/// Update flight DTO
/// </summary>
public class UpdateFlightDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public int TotalSeats { get; set; }
}