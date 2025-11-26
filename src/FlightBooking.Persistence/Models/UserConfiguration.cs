using FlightBooking.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlightBooking.Persistence.Models;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.HasIndex(e => e.Email)
            .IsUnique();
            
        builder.Property(e => e.FirstName)
            .HasMaxLength(50);
            
        builder.Property(e => e.LastName)
            .HasMaxLength(50);
            
        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(20);
            
        builder.Property(e => e.CreatedAt)
            .IsRequired();
            
        // Seed Admin User
        builder.HasData(
            new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@flightbooking.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                IsAdmin = true,
                IsGuest = false,
                CreatedAt = DateTime.UtcNow
            }
        );
    }
}
