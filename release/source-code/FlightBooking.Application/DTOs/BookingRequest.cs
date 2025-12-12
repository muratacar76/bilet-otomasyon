namespace FlightBooking.Application.DTOs;

public class BookingRequest
{
    public int FlightId { get; set; }
    public string? UserEmail { get; set; }
    public List<PassengerDto> Passengers { get; set; } = new();
}

public class PassengerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string? SeatNumber { get; set; }
    public string? SeatType { get; set; }
}
