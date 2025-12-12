using Microsoft.AspNetCore.Mvc;
using FlightBooking.Web.Models;
using System.Diagnostics;

namespace FlightBooking.Web.Controllers;

/// <summary>
/// Home controller for main pages
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Main landing page
    /// </summary>
    public IActionResult Index()
    {
        ViewData["Title"] = "Bulut Bilet.com - Hayalinizdeki Yolculuğa Başlayın";
        return View();
    }

    /// <summary>
    /// About page
    /// </summary>
    public IActionResult About()
    {
        ViewData["Title"] = "Hakkımızda - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Contact page
    /// </summary>
    public IActionResult Contact()
    {
        ViewData["Title"] = "İletişim - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Privacy policy page
    /// </summary>
    public IActionResult Privacy()
    {
        ViewData["Title"] = "Gizlilik Politikası - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Terms of service page
    /// </summary>
    public IActionResult Terms()
    {
        ViewData["Title"] = "Kullanım Şartları - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Campaigns page
    /// </summary>
    public IActionResult Campaigns()
    {
        ViewData["Title"] = "Kampanyalar - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// FAQ page
    /// </summary>
    public IActionResult FAQ()
    {
        ViewData["Title"] = "Sık Sorulan Sorular - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Help page
    /// </summary>
    public IActionResult Help()
    {
        ViewData["Title"] = "Yardım - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Error page
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel 
        { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}