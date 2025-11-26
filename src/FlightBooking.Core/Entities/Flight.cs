namespace FlightBooking.Core.Entities;

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
    public DateTime CreatedAt { get; set; }
    public int SeatsPerRow { get; set; } = 6; // Varsayılan: 6 koltuk (A-F)
    public int TotalRows { get; set; } = 30; // Varsayılan: 30 sıra
    
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
