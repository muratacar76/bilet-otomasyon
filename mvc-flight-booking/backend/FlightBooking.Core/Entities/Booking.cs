namespace FlightBooking.Core.Entities;

/// <summary>
/// Booking entity representing flight reservations
/// </summary>
public class Booking
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty; // PNR
    public int UserId { get; set; }
    public int FlightId { get; set; }
    public int PassengerCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed"; // Confirmed, Cancelled, Pending
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    public bool IsPaid { get; set; } = false;
    public DateTime? PaymentDate { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Flight Flight { get; set; } = null!;
    public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
    
    // Computed properties
    public bool CanBeCancelled => Status == "Confirmed" && 
                                  Flight?.DepartureTime > DateTime.UtcNow.AddHours(24);
    public decimal PricePerPassenger => PassengerCount > 0 ? TotalPrice / PassengerCount : 0;
    public string StatusDisplay => Status switch
    {
        "Confirmed" when IsPaid => "Ödendi",
        "Confirmed" when !IsPaid => "Onaylandı",
        "Cancelled" => "İptal Edildi",
        "Pending" => "Beklemede",
        _ => Status
    };
}