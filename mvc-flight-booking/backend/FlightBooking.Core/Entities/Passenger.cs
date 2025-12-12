namespace FlightBooking.Core.Entities;

/// <summary>
/// Passenger entity representing individual travelers in a booking
/// </summary>
public class Passenger
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty; // TC Kimlik No
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty; // Erkek, Kadın
    public string PhoneNumber { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty; // e.g., "12A"
    public string SeatType { get; set; } = string.Empty; // Window, Aisle, Middle
    
    // Navigation properties
    public virtual Booking Booking { get; set; } = null!;
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public int Age => DateTime.Now.Year - DateOfBirth.Year - 
                     (DateTime.Now.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    public string SeatTypeDisplay => SeatType switch
    {
        "Window" => "🪟 Cam Kenarı",
        "Aisle" => "🚶 Koridor",
        "Middle" => "💺 Orta",
        _ => SeatType
    };
}