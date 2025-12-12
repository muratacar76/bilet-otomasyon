using FlightBooking.Application.DTOs;

namespace FlightBooking.Application.Services;

/// <summary>
/// Flight service interface
/// </summary>
public interface IFlightService
{
    Task<IEnumerable<FlightDto>> SearchFlightsAsync(FlightSearchRequest request);
    Task<FlightDto?> GetFlightByIdAsync(int flightId);
    Task<FlightDto> CreateFlightAsync(FlightRequest request);
    Task<FlightDto> CreateFlightAsync(CreateFlightDto createFlightDto);
    Task<FlightDto> UpdateFlightAsync(int flightId, FlightRequest request);
    Task<FlightDto> UpdateFlightAsync(int flightId, UpdateFlightDto updateFlightDto);
    Task<IEnumerable<FlightDto>> GetAllFlightsAsync();
    Task<bool> DeleteFlightAsync(int flightId);
    Task<SeatMapResponse> GetSeatMapAsync(int flightId);
    Task<IEnumerable<FlightDto>> GetActiveFlightsAsync();
    Task<IEnumerable<FlightDto>> GetFlightsByAirlineAsync(string airline);
    Task<bool> UpdateFlightSeatsAsync(int flightId, int seatChange);
    Task GenerateSeatsForFlightAsync(int flightId);
}

/// <summary>
/// Seat service interface
/// </summary>
public interface ISeatService
{
    Task<IEnumerable<SeatDto>> GetFlightSeatsAsync(int flightId);
    Task<IEnumerable<SeatDto>> GetAvailableSeatsAsync(int flightId);
    Task<bool> IsSeatAvailableAsync(int flightId, string seatNumber);
    Task<bool> ReserveSeatAsync(int flightId, string seatNumber);
    Task<bool> ReleaseSeatAsync(int flightId, string seatNumber);
    Task<SeatDto?> GetSeatByNumberAsync(int flightId, string seatNumber);
    Task GenerateSeatsForFlightAsync(int flightId, int seatsPerRow, int totalRows);
}