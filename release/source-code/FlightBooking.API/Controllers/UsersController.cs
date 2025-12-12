using FlightBooking.Application.DTOs;
using FlightBooking.Core.Entities;
using FlightBooking.Persistence.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlightBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _context.Users
            .Include(u => u.Bookings)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.PhoneNumber,
                u.IdentityNumber,
                u.DateOfBirth,
                u.Gender,
                u.IsGuest,
                u.IsAdmin,
                u.CreatedAt,
                BookingCount = u.Bookings.Count,
                IsActive = !string.IsNullOrEmpty(u.PasswordHash) || u.IsGuest
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Bookings)
            .ThenInclude(b => b.Flight)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        return Ok(new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.IdentityNumber,
            user.DateOfBirth,
            user.Gender,
            user.IsGuest,
            user.IsAdmin,
            user.CreatedAt,
            Bookings = user.Bookings.Select(b => new
            {
                b.Id,
                b.BookingReference,
                b.TotalPrice,
                b.Status,
                b.IsPaid,
                b.BookingDate,
                FlightNumber = b.Flight?.FlightNumber,
                Route = $"{b.Flight?.DepartureCity} → {b.Flight?.ArrivalCity}"
            }),
            IsActive = !string.IsNullOrEmpty(user.PasswordHash) || user.IsGuest
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor" });

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            IdentityNumber = request.IdentityNumber,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            IsGuest = false,
            IsAdmin = request.IsAdmin,
            CreatedAt = DateTime.UtcNow
        };

        // Eğer şifre verilmişse hash'le
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanıcı başarıyla oluşturuldu", userId = user.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        // E-posta değişikliği kontrolü
        if (request.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
            return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor" });

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.IdentityNumber = request.IdentityNumber;
        user.DateOfBirth = request.DateOfBirth;
        user.Gender = request.Gender;
        user.IsAdmin = request.IsAdmin;

        // Şifre güncellemesi
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanıcı başarıyla güncellendi" });
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        // Admin kendini pasifleştiremez
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (user.Id == currentUserId)
            return BadRequest(new { message = "Kendi hesabınızı pasifleştiremezsiniz" });

        // Misafir kullanıcıları pasifleştirme/aktifleştirme
        if (user.IsGuest)
        {
            return BadRequest(new { message = "Misafir kullanıcıların durumu değiştirilemez" });
        }

        // Mevcut durumu kontrol et
        var currentlyActive = !string.IsNullOrEmpty(user.PasswordHash);
        
        if (currentlyActive)
        {
            // Aktifse pasifleştir
            user.PasswordHash = null;
        }
        else
        {
            // Pasifse aktifleştir - geçici şifre ver
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456");
        }

        await _context.SaveChangesAsync();

        var isActive = !string.IsNullOrEmpty(user.PasswordHash);
        return Ok(new { 
            message = isActive ? "Kullanıcı aktifleştirildi" : "Kullanıcı pasifleştirildi",
            isActive,
            tempPassword = isActive ? "123456" : null
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .Include(u => u.Bookings)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound(new { message = "Kullanıcı bulunamadı" });

        // Admin kendini silemez
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (user.Id == currentUserId)
            return BadRequest(new { message = "Kendi hesabınızı silemezsiniz" });

        // Rezervasyonu olan kullanıcıyı silmeden önce rezervasyonlarını da sil
        if (user.Bookings.Any())
        {
            // Önce passengers'ları sil
            var passengerIds = user.Bookings.SelectMany(b => _context.Passengers.Where(p => p.BookingId == b.Id).Select(p => p.Id)).ToList();
            var passengers = await _context.Passengers.Where(p => passengerIds.Contains(p.Id)).ToListAsync();
            _context.Passengers.RemoveRange(passengers);

            // Uçuşlardaki koltuk sayılarını geri yükle
            foreach (var booking in user.Bookings.Where(b => b.Status == "Confirmed"))
            {
                var flight = await _context.Flights.FindAsync(booking.FlightId);
                if (flight != null)
                {
                    flight.AvailableSeats += booking.PassengerCount;
                }
            }

            // Rezervasyonları sil
            _context.Bookings.RemoveRange(user.Bookings);
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Kullanıcı başarıyla silindi" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => !string.IsNullOrEmpty(u.PasswordHash) || u.IsGuest);
        var guestUsers = await _context.Users.CountAsync(u => u.IsGuest);
        var adminUsers = await _context.Users.CountAsync(u => u.IsAdmin);
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-30));

        return Ok(new
        {
            totalUsers,
            activeUsers,
            passiveUsers = totalUsers - activeUsers,
            guestUsers,
            adminUsers,
            newUsersThisMonth
        });
    }
}