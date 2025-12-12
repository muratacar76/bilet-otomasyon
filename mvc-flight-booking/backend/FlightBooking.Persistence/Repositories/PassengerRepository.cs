using FlightBooking.Core.Entities;
using FlightBooking.Core.Interfaces;
using FlightBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Repositories
{
    public class PassengerRepository : Repository<Passenger>, IPassengerRepository
    {
        public PassengerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Passenger>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Passengers
                .Where(p => p.BookingId == bookingId)
                .ToListAsync();
        }

        public async Task<Passenger?> GetByIdentityNumberAsync(string identityNumber)
        {
            return await _context.Passengers
                .FirstOrDefaultAsync(p => p.IdentityNumber == identityNumber);
        }
    }
}