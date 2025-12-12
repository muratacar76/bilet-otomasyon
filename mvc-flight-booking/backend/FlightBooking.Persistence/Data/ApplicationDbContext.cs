using FlightBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.Persistence.Data;

/// <summary>
/// Main database context class
/// Manages database operations with Entity Framework Core
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets - Database tables
    public DbSet<User> Users { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Passenger> Passengers { get; set; }
    public DbSet<Seat> Seats { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User entity configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.IdentityNumber).HasMaxLength(11);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IdentityNumber);
        });

        // Flight entity configuration
        modelBuilder.Entity<Flight>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FlightNumber).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Airline).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DepartureCity).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ArrivalCity).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Active");
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.CreatedAt).IsRequired();
            
            entity.HasIndex(e => e.FlightNumber);
            entity.HasIndex(e => new { e.DepartureCity, e.ArrivalCity });
            entity.HasIndex(e => e.DepartureTime);
        });

        // Booking entity configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BookingReference).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Onaylandı");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BookingDate).IsRequired();
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            
            entity.HasIndex(e => e.BookingReference).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.FlightId);
        });

        // Passenger entity configuration
        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IdentityNumber).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Gender).IsRequired().HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.SeatNumber).HasMaxLength(5);
            entity.Property(e => e.SeatType).HasMaxLength(10);
            
            entity.HasIndex(e => e.BookingId);
            entity.HasIndex(e => e.IdentityNumber);
        });

        // Seat entity configuration
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SeatNumber).IsRequired().HasMaxLength(5);
            entity.Property(e => e.SeatType).IsRequired().HasMaxLength(10);
            entity.Property(e => e.Column).IsRequired().HasMaxLength(1);
            
            entity.HasIndex(e => new { e.FlightId, e.SeatNumber }).IsUnique();
            entity.HasIndex(e => e.FlightId);
        });

        // Relationships
        
        // Booking - User relationship
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Booking - Flight relationship
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Flight)
            .WithMany(f => f.Bookings)
            .HasForeignKey(b => b.FlightId)
            .OnDelete(DeleteBehavior.Restrict);

        // Passenger - Booking relationship
        modelBuilder.Entity<Passenger>()
            .HasOne(p => p.Booking)
            .WithMany(b => b.Passengers)
            .HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seat - Flight relationship
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Flight)
            .WithMany(f => f.Seats)
            .HasForeignKey(s => s.FlightId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}