using Microsoft.AspNetCore.Mvc;
using FlightBooking.Core.Interfaces;
using FlightBooking.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FlightBooking.Web.Controllers.Api;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersApiController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ILogger<UsersApiController> _logger;

    public UsersApiController(
        IUserRepository userRepository,
        IBookingRepository bookingRepository,
        ILogger<UsersApiController> logger)
    {
        _userRepository = userRepository;
        _bookingRepository = bookingRepository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            
            var result = new List<object>();
            foreach (var user in users)
            {
                var bookings = await _bookingRepository.GetByUserIdAsync(user.Id);
                result.Add(new
                {
                    id = user.Id,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    email = user.Email,
                    phoneNumber = user.PhoneNumber,
                    identityNumber = user.IdentityNumber,
                    dateOfBirth = user.DateOfBirth,
                    gender = user.Gender,
                    isActive = user.IsActive,
                    isAdmin = user.IsAdmin,
                    isGuest = user.IsGuest,
                    createdAt = user.CreatedAt,
                    bookingCount = bookings.Count()
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "Kullanıcılar yüklenirken hata oluştu");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null && !existingUser.IsGuest)
            {
                return BadRequest("Bu e-posta adresi zaten kullanılıyor");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = !string.IsNullOrEmpty(request.Password) ? BCrypt.Net.BCrypt.HashPassword(request.Password) : null,
                PhoneNumber = request.PhoneNumber,
                IdentityNumber = request.IdentityNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                IsGuest = false,
                IsAdmin = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return StatusCode(500, "Kullanıcı oluşturulurken hata oluştu");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }
            user.PhoneNumber = request.PhoneNumber;
            user.IdentityNumber = request.IdentityNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return Ok(new { message = "Kullanıcı başarıyla güncellendi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return StatusCode(500, "Kullanıcı güncellenirken hata oluştu");
        }
    }

    [HttpPatch("{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı");
            }

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            
            var statusText = user.IsActive ? "aktifleştirildi" : "pasifleştirildi";
            return Ok(new { message = $"Kullanıcı başarıyla {statusText}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling user status {UserId}", id);
            return StatusCode(500, "Kullanıcı durumu değiştirilirken hata oluştu");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (id == currentUserId)
            {
                return BadRequest("Kendi hesabınızı silemezsiniz");
            }

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı");
            }

            // Delete user's bookings first
            var bookings = await _bookingRepository.GetByUserIdAsync(id);
            foreach (var booking in bookings)
            {
                await _bookingRepository.DeleteAsync(booking);
            }

            await _userRepository.DeleteAsync(user);
            return Ok(new { message = "Kullanıcı ve rezervasyonları başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "Kullanıcı silinirken hata oluştu");
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var oneMonthAgo = DateTime.UtcNow.AddDays(-30);

            var stats = new
            {
                totalUsers = users.Count(),
                activeUsers = users.Count(u => u.IsActive),
                passiveUsers = users.Count(u => !u.IsActive),
                guestUsers = users.Count(u => u.IsGuest),
                adminUsers = users.Count(u => u.IsAdmin),
                newUsersThisMonth = users.Count(u => u.CreatedAt >= oneMonthAgo)
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user stats");
            return StatusCode(500, "İstatistikler yüklenirken hata oluştu");
        }
    }
}

public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Password { get; set; }
    public string? PhoneNumber { get; set; }
    public string? IdentityNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
}