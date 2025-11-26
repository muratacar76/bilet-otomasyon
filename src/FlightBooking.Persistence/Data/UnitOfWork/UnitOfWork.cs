using FlightBooking.Core.Entities;
using FlightBooking.Persistence.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace FlightBooking.Persistence.Data.UnitOfWork;

/// <summary>
/// Unit of Work pattern implementation
/// Tüm repository'leri yönetir ve transaction desteği sağlar
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        Flights = new Repository<Flight>(_context);
        Bookings = new Repository<Booking>(_context);
        Passengers = new Repository<Passenger>(_context);
    }

    public IRepository<User> Users { get; }
    public IRepository<Flight> Flights { get; }
    public IRepository<Booking> Bookings { get; }
    public IRepository<Passenger> Passengers { get; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
