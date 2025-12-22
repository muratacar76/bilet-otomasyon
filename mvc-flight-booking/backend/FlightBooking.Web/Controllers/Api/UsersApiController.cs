using Microsoft.AspNetCore.Mvc;
using FlightBooking.Core.Interfaces;
using FlightBooking.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FlightBooking.Web.Controllers.Api;

/// <summary>
/// Kullanıcı yönetimi için API Controller
/// Admin panelinden kullanıcı işlemlerini yönetir
/// </summary>
[ApiController]
[Route("api/users")]
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

    /// <summary>
    /// Tüm kullanıcıları getirir (Sadece admin)
    /// </summary>
    [HttpGet]
    [Authorize] // Giriş yapmış kullanıcı gerekli
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            _logger.LogInformation("GetUsers API called. Authenticated: {IsAuthenticated}", User.Identity.IsAuthenticated);
            
            // Kullanıcı giriş yapmış mı kontrol et
            if (!User.Identity.IsAuthenticated)
            {
                _logger.LogWarning("Unauthenticated user attempted to access users API");
                return Unauthorized("Giriş yapmanız gereklidir");
            }
            
            // Admin kontrolü - farklı claim türlerini kontrol et
            var isAdmin = User.FindFirst("IsAdmin")?.Value == "true" || 
                         User.IsInRole("Admin") || 
                         User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
            
            _logger.LogInformation("GetUsers API called. User: {UserId}, IsAdmin: {IsAdmin}, Claims: {Claims}", 
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                isAdmin,
                string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
            
            // Admin değilse erişimi reddet
            if (!isAdmin)
            {
                _logger.LogWarning("Non-admin user attempted to access users API: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                return StatusCode(403, "Bu işlem için admin yetkisi gereklidir");
            }

            _logger.LogInformation("Fetching users from repository...");
            var users = await _userRepository.GetAllAsync();
            _logger.LogInformation("Found {UserCount} users", users.Count());
            
            // Kullanıcı listesini hazırla
            var result = new List<object>();
            foreach (var user in users)
            {
                try
                {
                    // Her kullanıcının rezervasyon sayısını al
                    var bookings = await _bookingRepository.GetByUserIdAsync(user.Id);
                    result.Add(new
                    {
                        id = user.Id,
                        firstName = user.FirstName ?? "",
                        lastName = user.LastName ?? "",
                        email = user.Email,
                        phoneNumber = user.PhoneNumber ?? "",
                        identityNumber = user.IdentityNumber ?? "",
                        dateOfBirth = user.DateOfBirth,
                        gender = user.Gender ?? "",
                        isActive = user.IsActive,
                        isAdmin = user.IsAdmin,
                        isGuest = user.IsGuest,
                        createdAt = user.CreatedAt,
                        bookingCount = bookings.Count()
                    });
                }
                catch (Exception userEx)
                {
                    _logger.LogError(userEx, "Error processing user {UserId}", user.Id);
                }
            }

            _logger.LogInformation("Returning {ResultCount} users", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return StatusCode(500, "Kullanıcılar yüklenirken hata oluştu");
        }
    }

    /// <summary>
    /// Yeni kullanıcı oluşturur
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // E-posta zaten kullanılıyor mu kontrol et
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null && !existingUser.IsGuest)
            {
                return BadRequest("Bu e-posta adresi zaten kullanılıyor");
            }

            // Yeni kullanıcı oluştur
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

    /// <summary>
    /// Kullanıcı bilgilerini günceller
    /// </summary>
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

            // Kullanıcı bilgilerini güncelle
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

    /// <summary>
    /// Kullanıcı durumunu aktif/pasif yapar
    /// </summary>
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

            // Durumu tersine çevir
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

    /// <summary>
    /// Kullanıcıyı siler
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            // Kendi hesabını silmeye çalışıyor mu kontrol et
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

            // Önce kullanıcının rezervasyonlarını sil
            var bookings = await _bookingRepository.GetByUserIdAsync(id);
            foreach (var booking in bookings)
            {
                await _bookingRepository.DeleteAsync(booking);
            }

            // Sonra kullanıcıyı sil
            await _userRepository.DeleteAsync(user);
            return Ok(new { message = "Kullanıcı ve rezervasyonları başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, "Kullanıcı silinirken hata oluştu");
        }
    }

    /// <summary>
    /// Kullanıcı istatistiklerini getirir
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetUserStats()
    {
        try
        {
            var users = await _userRepository.GetAllAsync();
            var oneMonthAgo = DateTime.UtcNow.AddDays(-30);

            // İstatistikleri hesapla
            var stats = new
            {
                totalUsers = users.Count(), // Toplam kullanıcı
                activeUsers = users.Count(u => u.IsActive), // Aktif kullanıcı
                passiveUsers = users.Count(u => !u.IsActive), // Pasif kullanıcı
                guestUsers = users.Count(u => u.IsGuest), // Misafir kullanıcı
                adminUsers = users.Count(u => u.IsAdmin), // Admin kullanıcı
                newUsersThisMonth = users.Count(u => u.CreatedAt >= oneMonthAgo) // Bu ay kayıt olan
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

/// <summary>
/// Kullanıcı oluşturma/güncelleme için request modeli
/// </summary>
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