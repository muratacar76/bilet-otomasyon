namespace FlightBooking.Core.Entities;

public class Passenger
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
    
    // Navigation property
    public Booking? Booking { get; set; }
}
