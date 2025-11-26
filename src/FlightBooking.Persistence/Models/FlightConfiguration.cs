using FlightBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBooking.Persistence.Models;

public class FlightConfiguration : IEntityTypeConfiguration<Flight>
{
    public void Configure(EntityTypeBuilder<Flight> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FlightNumber)
            .IsRequired()
            .HasMaxLength(20);
            
        builder.Property(e => e.Airline)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.DepartureCity)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.ArrivalCity)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Price)
            .HasColumnType("decimal(18,2)")
            .IsRequired();
            
        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("Active");
            
        builder.Property(e => e.DepartureTime)
            .IsRequired();
            
        builder.Property(e => e.ArrivalTime)
            .IsRequired();
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        builder.Property(e => e.SeatsPerRow)
            .IsRequired()
            .HasDefaultValue(6);
            
        builder.Property(e => e.TotalRows)
            .IsRequired()
            .HasDefaultValue(30);
            
        builder.HasIndex(e => e.FlightNumber);
        builder.HasIndex(e => e.DepartureTime);
        builder.HasIndex(e => new { e.DepartureCity, e.ArrivalCity });
    }
}
