using System.Linq.Expressions;

namespace FlightBooking.Core.Interfaces;

/// <summary>
/// Generic repository interface for basic CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository : IRepository<Entities.User>
{
    Task<Entities.User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<IEnumerable<Entities.User>> GetAdminsAsync();
    Task<IEnumerable<Entities.User>> GetGuestUsersAsync();
}

/// <summary>
/// Flight repository interface
/// </summary>
public interface IFlightRepository : IRepository<Entities.Flight>
{
    Task<IEnumerable<Entities.Flight>> SearchFlightsAsync(
        string? departureCity = null,
        string? arrivalCity = null,
        DateTime? departureDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);
    Task<Entities.Flight?> GetFlightWithSeatsAsync(int flightId);
    Task<IEnumerable<Entities.Flight>> GetActiveFlightsAsync();
    Task<IEnumerable<Entities.Flight>> GetFlightsByAirlineAsync(string airline);
}

/// <summary>
/// Booking repository interface
/// </summary>
public interface IBookingRepository : IRepository<Entities.Booking>
{
    Task<Entities.Booking?> GetByReferenceAsync(string bookingReference);
    Task<IEnumerable<Entities.Booking>> GetUserBookingsAsync(int userId);
    Task<IEnumerable<Entities.Booking>> GetFlightBookingsAsync(int flightId);
    Task<Entities.Booking?> GetBookingWithDetailsAsync(int bookingId);
    Task<IEnumerable<Entities.Booking>> GetBookingsByEmailAsync(string email);
    Task<IEnumerable<Entities.Booking>> GetPendingPaymentsAsync();
    Task<Entities.Booking?> GetByPnrAndEmailAsync(string pnr, string email);
    Task<IEnumerable<Entities.Booking>> GetByUserIdAsync(int userId);
    Task<bool> ExistsByReferenceAsync(string bookingReference);
}

/// <summary>
/// Passenger repository interface
/// </summary>
public interface IPassengerRepository : IRepository<Entities.Passenger>
{
    Task<IEnumerable<Entities.Passenger>> GetByBookingIdAsync(int bookingId);
    Task<Entities.Passenger?> GetByIdentityNumberAsync(string identityNumber);
    Task DeleteByBookingIdAsync(int bookingId);
}

/// <summary>
/// Seat repository interface
/// </summary>
public interface ISeatRepository : IRepository<Entities.Seat>
{
    Task<IEnumerable<Entities.Seat>> GetFlightSeatsAsync(int flightId);
    Task<IEnumerable<Entities.Seat>> GetAvailableSeatsAsync(int flightId);
    Task<Entities.Seat?> GetSeatByNumberAsync(int flightId, string seatNumber);
    Task<bool> IsSeatAvailableAsync(int flightId, string seatNumber);
}