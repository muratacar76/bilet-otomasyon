namespace FlightBooking.Core.Entities;

/// <summary>
/// Flight entity representing flight information
/// </summary>
public class Flight
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
    public string Status { get; set; } = "Active";
    public int SeatsPerRow { get; set; } = 6; // Default: 6 seats (A-F)
    public int TotalRows { get; set; } = 30; // Default: 30 rows
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<Seat> Seats { get; set; } = new List<Seat>();
    
    // Computed properties
    public string Route => $"{DepartureCity} → {ArrivalCity}";
    public TimeSpan FlightDuration => ArrivalTime - DepartureTime;
    public bool IsAvailable => Status == "Active" && AvailableSeats > 0;
    public int BookedSeats => TotalSeats - AvailableSeats;
}