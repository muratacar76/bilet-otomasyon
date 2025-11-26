using FlightBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBooking.Persistence.Models;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.BookingReference)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.HasIndex(e => e.BookingReference)
            .IsUnique();
            
        builder.Property(e => e.TotalPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Pending");
            
        builder.Property(e => e.BookingDate)
            .IsRequired();
            
        // İlişkiler
        builder.HasOne(e => e.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(e => e.Flight)
            .WithMany(f => f.Bookings)
            .HasForeignKey(e => e.FlightId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.FlightId);
        builder.HasIndex(e => e.BookingDate);
    }
}
