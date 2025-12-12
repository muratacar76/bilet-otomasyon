using FlightBooking.Core.Entities;
using FlightBooking.Persistence.Data.Repositories;

namespace FlightBooking.Persistence.Data.UnitOfWork;

/// <summary>
/// Unit of Work pattern interface
/// Tüm repository'leri ve transaction yönetimini sağlar
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Flight> Flights { get; }
    IRepository<Booking> Bookings { get; }
    IRepository<Passenger> Passengers { get; }
    
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
