namespace FlightBooking.Application.DTOs;

/// <summary>
/// Booking creation request DTO
/// </summary>
public class BookingRequest
{
    public int FlightId { get; set; }
    public List<PassengerDto> Passengers { get; set; } = new();
    public string? GuestEmail { get; set; }
}

/// <summary>
/// Passenger DTO
/// </summary>
public class PassengerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string IdentityNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string SeatNumber { get; set; } = string.Empty;
    public string SeatType { get; set; } = string.Empty;
    
    // Computed properties
    public string FullName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string SeatTypeDisplay { get; set; } = string.Empty;
}

/// <summary>
/// Booking DTO for responses
/// </summary>
public class BookingDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public int UserId { get; set; }
    public int FlightId { get; set; }
    public int PassengerCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public bool IsPaid { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? CancellationDate { get; set; }
    public string? CancellationReason { get; set; }
    
    // Navigation properties
    public UserDto? User { get; set; }
    public FlightDto? Flight { get; set; }
    public List<PassengerDto> Passengers { get; set; } = new();
    
    // Computed properties
    public bool CanBeCancelled { get; set; }
    public decimal PricePerPassenger { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
}

/// <summary>
/// Booking summary DTO for lists
/// </summary>
public class BookingSummaryDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public int PassengerCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public DateTime BookingDate { get; set; }
}

/// <summary>
/// Payment request DTO
/// </summary>
public class PaymentRequest
{
    public int BookingId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // CreditCard, BankTransfer, etc.
    public decimal Amount { get; set; }
    public string? CardNumber { get; set; }
    public string? CardHolderName { get; set; }
    public string? ExpiryDate { get; set; }
    public string? CVV { get; set; }
}

/// <summary>
/// Payment response DTO
/// </summary>
public class PaymentResponse
{
    public bool IsSuccess { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
}

/// <summary>
/// Booking cancellation request DTO
/// </summary>
public class CancellationRequest
{
    public int BookingId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// PNR query request DTO
/// </summary>
public class PNRQueryRequest
{
    public string PNR { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Delete all bookings result DTO
/// </summary>
public class DeleteAllBookingsResult
{
    public int DeletedCount { get; set; }
    public int DeletedPassengers { get; set; }
}