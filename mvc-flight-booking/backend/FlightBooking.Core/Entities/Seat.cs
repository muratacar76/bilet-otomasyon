namespace FlightBooking.Core.Entities;

/// <summary>
/// Seat entity representing individual seats in a flight
/// </summary>
public class Seat
{
    public int Id { get; set; }
    public int FlightId { get; set; }
    public string SeatNumber { get; set; } = string.Empty; // e.g., "12A"
    public string SeatType { get; set; } = string.Empty; // Window, Aisle, Middle
    public bool IsOccupied { get; set; } = false;
    public int Row { get; set; }
    public string Column { get; set; } = string.Empty; // A, B, C, D, E, F
    public bool IsExitRow { get; set; } = false;
    public bool IsBlocked { get; set; } = false; // For maintenance or other reasons
    
    // Navigation properties
    public virtual Flight Flight { get; set; } = null!;
    
    // Computed properties
    public bool IsAvailable => !IsOccupied && !IsBlocked;
    public string SeatTypeDisplay => SeatType switch
    {
        "Window" => "🪟 Cam Kenarı",
        "Aisle" => "🚶 Koridor", 
        "Middle" => "💺 Orta",
        _ => SeatType
    };
    public string ExitRowInfo => IsExitRow ? " (Çıkış Sırası - Ekstra Bacak Mesafesi)" : "";
}