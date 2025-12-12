using FlightBooking.Core.Entities;
using FlightBooking.Core.Interfaces;
using FlightBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<IEnumerable<User>> GetAdminsAsync()
    {
        return await _context.Users
            .Where(u => u.IsAdmin)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetGuestUsersAsync()
    {
        return await _context.Users
            .Where(u => u.IsGuest)
            .ToListAsync();
    }
}