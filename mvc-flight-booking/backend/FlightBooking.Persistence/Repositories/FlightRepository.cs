using FlightBooking.Core.Entities;
using FlightBooking.Core.Interfaces;
using FlightBooking.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Repositories;

public class FlightRepository : Repository<Flight>, IFlightRepository
{
    public FlightRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Flight>> SearchFlightsAsync(
        string? departureCity = null,
        string? arrivalCity = null,
        DateTime? departureDate = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        var query = _context.Flights.AsQueryable();

        if (!string.IsNullOrEmpty(departureCity))
        {
            query = query.Where(f => f.DepartureCity.Contains(departureCity));
        }

        if (!string.IsNullOrEmpty(arrivalCity))
        {
            query = query.Where(f => f.ArrivalCity.Contains(arrivalCity));
        }

        if (departureDate.HasValue)
        {
            var date = departureDate.Value.Date;
            query = query.Where(f => f.DepartureTime.Date == date);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(f => f.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(f => f.Price <= maxPrice.Value);
        }

        return await query
            .Where(f => f.Status == "Active")
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();
    }

    public async Task<Flight?> GetFlightWithSeatsAsync(int flightId)
    {
        return await _context.Flights
            .Include(f => f.Seats)
            .FirstOrDefaultAsync(f => f.Id == flightId);
    }

    public async Task<IEnumerable<Flight>> GetActiveFlightsAsync()
    {
        return await _context.Flights
            .Where(f => f.Status == "Active")
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Flight>> GetFlightsByAirlineAsync(string airline)
    {
        return await _context.Flights
            .Where(f => f.Airline == airline && f.Status == "Active")
            .OrderBy(f => f.DepartureTime)
            .ToListAsync();
    }
}