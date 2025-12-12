namespace FlightBooking.Application.DTOs;

public class FlightSearchRequest
{
    public string? DepartureCity { get; set; }
    public string? ArrivalCity { get; set; }
    public DateTime? DepartureDate { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }
}
