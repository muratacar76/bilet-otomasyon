using FlightBooking.Core.Entities;
using FlightBooking.Core.Interfaces;
using FlightBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Repositories;

public class SeatRepository : Repository<Seat>, ISeatRepository
{
    public SeatRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Seat>> GetFlightSeatsAsync(int flightId)
    {
        return await _context.Seats
            .Where(s => s.FlightId == flightId)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Column)
            .ToListAsync();
    }

    public async Task<IEnumerable<Seat>> GetAvailableSeatsAsync(int flightId)
    {
        return await _context.Seats
            .Where(s => s.FlightId == flightId && !s.IsOccupied && !s.IsBlocked)
            .OrderBy(s => s.Row)
            .ThenBy(s => s.Column)
            .ToListAsync();
    }

    public async Task<Seat?> GetSeatByNumberAsync(int flightId, string seatNumber)
    {
        return await _context.Seats
            .FirstOrDefaultAsync(s => s.FlightId == flightId && s.SeatNumber == seatNumber);
    }

    public async Task<bool> IsSeatAvailableAsync(int flightId, string seatNumber)
    {
        var seat = await GetSeatByNumberAsync(flightId, seatNumber);
        return seat != null && !seat.IsOccupied && !seat.IsBlocked;
    }
}