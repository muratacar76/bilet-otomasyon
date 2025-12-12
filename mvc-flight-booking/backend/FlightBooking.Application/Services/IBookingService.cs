using FlightBooking.Application.DTOs;

namespace FlightBooking.Application.Services;

/// <summary>
/// Booking service interface
/// </summary>
public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(BookingRequest request, int? userId = null);
    Task<BookingDto?> GetBookingByIdAsync(int bookingId, int? userId = null);
    Task<BookingDto?> GetBookingByReferenceAsync(string bookingReference, string email);
    Task<IEnumerable<BookingDto>> GetUserBookingsAsync(int userId);
    Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
    Task<IEnumerable<BookingSummaryDto>> GetBookingsByEmailAsync(string email);
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
    Task<bool> CancelBookingAsync(int bookingId, CancellationRequest request, int? userId = null);
    Task<DeleteAllBookingsResult> DeleteAllBookingsAsync();
    Task<string> GenerateBookingReferenceAsync();
    Task<bool> ValidateBookingAsync(BookingRequest request);
}

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    Task SendBookingConfirmationAsync(BookingDto booking);
    Task SendPaymentConfirmationAsync(BookingDto booking);
    Task SendCancellationNotificationAsync(BookingDto booking);
    Task SendWelcomeEmailAsync(UserDto user);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
    Task SendBookingConfirmationAsync(string email, string pnr, Core.Entities.Flight flight, List<string> passengerNames);
}