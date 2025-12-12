namespace FlightBooking.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public bool IsGuest { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public List<Booking> Bookings { get; set; } = new();
}
