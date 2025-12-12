namespace FlightBooking.Web.Models;

/// <summary>
/// Error view model
/// </summary>
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}