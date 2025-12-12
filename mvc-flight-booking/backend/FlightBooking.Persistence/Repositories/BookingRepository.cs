using FlightBooking.Core.Entities;
using FlightBooking.Core.Interfaces;
using FlightBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Repositories;

/// <summary>
/// Booking repository implementation
/// </summary>
public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public BookingRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Booking?> GetByReferenceAsync(string bookingReference)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .FirstOrDefaultAsync(b => b.BookingReference.ToUpper() == bookingReference.ToUpper());
    }

    public async Task<IEnumerable<Booking>> GetUserBookingsAsync(int userId)
    {
        return await _dbSet
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<Booking?> GetByPnrAndEmailAsync(string pnr, string email)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .FirstOrDefaultAsync(b => b.BookingReference.ToUpper() == pnr.ToUpper() && 
                                    b.User.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<Booking>> GetFlightBookingsAsync(int flightId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Passengers)
            .Where(b => b.FlightId == flightId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<Booking?> GetBookingWithDetailsAsync(int bookingId)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
    }

    public async Task<IEnumerable<Booking>> GetBookingsByEmailAsync(string email)
    {
        return await _dbSet
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .Include(b => b.User)
            .Where(b => b.User != null && b.User.Email.ToLower() == email.ToLower())
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetPendingPaymentsAsync()
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Flight)
            .Where(b => b.Status == "Confirmed" && !b.IsPaid)
            .OrderBy(b => b.BookingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
    {
        return await GetUserBookingsAsync(userId);
    }

    public override async Task<IEnumerable<Booking>> GetAllAsync()
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    public override async Task<Booking?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(b => b.User)
            .Include(b => b.Flight)
            .Include(b => b.Passengers)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetByIdWithDetailsAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<Booking?> GetByReferenceWithDetailsAsync(string bookingReference)
    {
        return await GetByReferenceAsync(bookingReference);
    }

    public async Task<IEnumerable<Booking>> GetAllWithDetailsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<bool> ExistsByReferenceAsync(string bookingReference)
    {
        return await _dbSet
            .AnyAsync(b => b.BookingReference.ToUpper() == bookingReference.ToUpper());
    }}
