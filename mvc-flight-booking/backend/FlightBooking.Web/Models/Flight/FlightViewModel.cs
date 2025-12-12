namespace FlightBooking.Web.Models.Flight;

/// <summary>
/// Flight view model
/// </summary>
public class FlightViewModel
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
    public string Status { get; set; } = string.Empty;
    
    // Computed properties
    public string Route => $"{DepartureCity} → {ArrivalCity}";
    public TimeSpan FlightDuration => ArrivalTime - DepartureTime;
    public bool IsAvailable => Status == "Active" && AvailableSeats > 0;
    public int BookedSeats => TotalSeats - AvailableSeats;
    public string FormattedPrice => Price.ToString("C", new System.Globalization.CultureInfo("tr-TR"));
    public string FormattedDepartureTime => DepartureTime.ToString("dd.MM.yyyy HH:mm");
    public string FormattedArrivalTime => ArrivalTime.ToString("dd.MM.yyyy HH:mm");
    public string FormattedDuration => $"{FlightDuration.Hours}s {FlightDuration.Minutes}dk";
}