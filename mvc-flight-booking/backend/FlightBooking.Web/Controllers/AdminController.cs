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
        // Admin kontrolü
        if (!User.IsInRole("Admin") && User.FindFirst("IsAdmin")?.Value != "true")
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Kullanıcı Yönetimi - Admin Panel";
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
}