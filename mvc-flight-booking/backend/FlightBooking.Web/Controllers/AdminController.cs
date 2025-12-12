using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Web.Controllers;

/// <summary>
/// Admin panel controller
/// </summary>
[Authorize]
public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;

    public AdminController(ILogger<AdminController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Admin dashboard
    /// </summary>
    public IActionResult Index()
    {
        // Admin kontrolü
        if (!User.IsInRole("Admin") && User.FindFirst("IsAdmin")?.Value != "true")
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Admin Panel - BULUTBİLET.COM";
        return View();
    }

    /// <summary>
    /// Flight management page
    /// </summary>
    public IActionResult Flights()
    {
        // Admin kontrolü
        if (!User.IsInRole("Admin") && User.FindFirst("IsAdmin")?.Value != "true")
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Uçuş Yönetimi - Admin Panel";
        return View();
    }

    /// <summary>
    /// User management page
    /// </summary>
    public IActionResult Users()
    {
        // Admin kontrolü - daha esnek kontrol
        var isAdmin = User.IsInRole("Admin") || 
                     User.FindFirst("IsAdmin")?.Value == "true" ||
                     User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
        
        _logger.LogInformation("Users page accessed. User: {UserId}, IsAdmin: {IsAdmin}, Claims: {Claims}", 
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            isAdmin,
            string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}")));
        
        if (!isAdmin)
        {
            _logger.LogWarning("Non-admin user attempted to access Users page: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            TempData["ErrorMessage"] = "Bu sayfaya erişim yetkiniz bulunmamaktadır.";
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Kullanıcı Yönetimi - Admin Panel";
        ViewBag.CurrentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        ViewBag.IsAdmin = isAdmin;
        return View();
    }

    /// <summary>
    /// Booking management page
    /// </summary>
    public IActionResult Bookings()
    {
        // Admin kontrolü
        if (!User.IsInRole("Admin") && User.FindFirst("IsAdmin")?.Value != "true")
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Rezervasyon Yönetimi - Admin Panel";
        return View();
    }

    /// <summary>
    /// Reset database (for testing)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ResetDatabase([FromServices] FlightBooking.Persistence.Data.ApplicationDbContext context)
    {
        // Admin kontrolü
        var isAdmin = User.IsInRole("Admin") || 
                     User.FindFirst("IsAdmin")?.Value == "true" ||
                     User.FindFirst(ClaimTypes.Role)?.Value == "Admin";
        
        if (!isAdmin)
        {
            return Forbid();
        }

        try
        {
            // Delete database and recreate
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            
            // Re-initialize with seed data
            await FlightBooking.Persistence.Data.DbInitializer.InitializeAsync(context);
            
            TempData["Success"] = "Veritabanı başarıyla sıfırlandı ve test verileri eklendi.";
            _logger.LogInformation("Database reset by admin user: {UserId}", User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting database");
            TempData["Error"] = "Veritabanı sıfırlanırken hata oluştu: " + ex.Message;
        }

        return RedirectToAction("Index");
    }
}