namespace FlightBooking.Core.Enums;

/// <summary>
/// Booking status enumeration
/// </summary>
public enum BookingStatus
{
    Pending = 0,
    Confirmed = 1,
    Paid = 2,
    Cancelled = 3,
    Refunded = 4
}

/// <summary>
/// Flight status enumeration
/// </summary>
public enum FlightStatus
{
    Active = 0,
    Cancelled = 1,
    Delayed = 2,
    Completed = 3,
    Boarding = 4
}

/// <summary>
/// Seat type enumeration
/// </summary>
public enum SeatType
{
    Window = 0,
    Aisle = 1,
    Middle = 2
}

/// <summary>
/// Gender enumeration
/// </summary>
public enum Gender
{
    Male = 0,    // Erkek
    Female = 1   // Kadın
}