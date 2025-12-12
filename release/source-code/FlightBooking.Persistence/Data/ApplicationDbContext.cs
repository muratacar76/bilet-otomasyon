using FlightBooking.Core.Entities;
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
        
        // Booking - User relationship
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        // Booking - Flight relationship
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Flight)
            .WithMany(f => f.Bookings)
            .HasForeignKey(b => b.FlightId)
            .OnDelete(DeleteBehavior.NoAction);

        // Passenger - Booking relationship
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Passengers)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for better performance
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Flight>()
            .HasIndex(f => f.FlightNumber);

        modelBuilder.Entity<Booking>()
            .HasIndex(b => b.BookingReference)
            .IsUnique();
    }
}
