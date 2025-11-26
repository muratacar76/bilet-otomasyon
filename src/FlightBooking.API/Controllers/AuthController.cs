using FlightBooking.Application.DTOs;
using FlightBooking.Core.Entities;
using FlightBooking.Infrastructure.Services;
using FlightBooking.Persistence.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlightBooking.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;

    public AuthController(ApplicationDbContext context, JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor" });

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            IsGuest = false,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.FirstName, user.LastName } });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null || user.PasswordHash == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "E-posta veya şifre hatalı" });

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token, user = new { user.Id, user.Email, user.FirstName, user.LastName, user.IsAdmin } });
    }

    [HttpPost("guest")]
    public async Task<IActionResult> GuestLogin([FromBody] string email)
    {
        // Önce bu email ile kullanıcı var mı kontrol et
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (existingUser != null)
        {
            // Varsa mevcut kullanıcıyı kullan
            var existingToken = _jwtService.GenerateToken(existingUser);
            return Ok(new { token = existingToken, user = new { existingUser.Id, existingUser.Email } });
        }

        // Yoksa yeni misafir kullanıcı oluştur
        var user = new User
        {
            Email = email,
            IsGuest = true,
            IsAdmin = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);
        return Ok(new { token, user = new { user.Id, user.Email } });
    }
}
