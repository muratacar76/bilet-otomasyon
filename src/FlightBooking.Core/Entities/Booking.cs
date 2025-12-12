namespace FlightBooking.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int FlightId { get; set; }
    public int PassengerCount { get; set; }
    public double TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed";
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public bool IsPaid { get; set; } = false;
    public DateTime? PaymentDate { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    public Flight? Flight { get; set; }
    public List<Passenger> Passengers { get; set; } = new();
}
