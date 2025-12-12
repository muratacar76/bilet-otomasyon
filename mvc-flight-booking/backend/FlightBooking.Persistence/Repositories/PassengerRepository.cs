using FlightBooking.Core.Entities;
using FlightBooking.Core.Interfaces;
using FlightBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Repositories;

/// <summary>
/// Repository implementation for passenger operations
/// </summary>
public class PassengerRepository : Repository<Passenger>, IPassengerRepository
{
    public PassengerRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get passengers by booking ID
    /// </summary>
    public async Task<IEnumerable<Passenger>> GetByBookingIdAsync(int bookingId)
    {
        return await _context.Passengers
            .Where(p => p.BookingId == bookingId)
            .OrderBy(p => p.Id)
            .ToListAsync();
    }

    /// <summary>
    /// Get passenger by identity number
    /// </summary>
    public async Task<Passenger?> GetByIdentityNumberAsync(string identityNumber)
    {
        return await _context.Passengers
            .FirstOrDefaultAsync(p => p.IdentityNumber == identityNumber);
    }

    /// <summary>
    /// Delete passengers by booking ID
    /// </summary>
    public async Task DeleteByBookingIdAsync(int bookingId)
    {
        var passengers = await _context.Passengers
            .Where(p => p.BookingId == bookingId)
            .ToListAsync();

        _context.Passengers.RemoveRange(passengers);
        await _context.SaveChangesAsync();
    }
}