using FlightBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBooking.Persistence.Models;

public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
{
    public void Configure(EntityTypeBuilder<Passenger> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.IdentityNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(e => e.Gender)
            .IsRequired()
            .HasMaxLength(10);
            
        builder.Property(e => e.DateOfBirth)
            .IsRequired();
            
        builder.Property(e => e.SeatNumber)
            .HasMaxLength(10);
            
        builder.Property(e => e.SeatType)
            .HasMaxLength(20);
            
        // İlişki
        builder.HasOne(e => e.Booking)
            .WithMany(b => b.Passengers)
            .HasForeignKey(e => e.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(e => e.BookingId);
    }
}
