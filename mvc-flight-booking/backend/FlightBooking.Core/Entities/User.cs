namespace FlightBooking.Core.Entities;

/// <summary>
/// User entity representing system users (registered and guest users)
/// </summary>
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
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsRegistered => !IsGuest && !string.IsNullOrEmpty(PasswordHash);
}