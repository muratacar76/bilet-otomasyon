using FlightBooking.Core.Entities;
using FlightBooking.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Data;

/// <summary>
/// Ana veritabanı context sınıfı
/// Entity Framework Core ile veritabanı işlemlerini yönetir
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSet'ler - Veritabanı tabloları
    public DbSet<User> Users { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Passenger> Passengers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Model yapılandırmalarını uygula
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new FlightConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new PassengerConfiguration());
    }
}
