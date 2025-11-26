namespace FlightBooking.Core.Entities;

public class Booking
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int FlightId { get; set; }
    public int PassengerCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime BookingDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    
    public User User { get; set; } = null!;
    public Flight Flight { get; set; } = null!;
    public ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
}
