using System.Security.Claims;
using AutoMapper;
using FlightBooking.Application.DTOs;
using FlightBooking.Core.Interfaces;
using FlightBooking.Web.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlightBooking.Web.Controllers;

/// <summary>
/// Authentication controller
/// </summary>
public class AuthController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IMapper mapper,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Login page
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Giriş Yap - Bulut Bilet.com";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Process login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["Title"] = "Giriş Yap - Bulut Bilet.com";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(model.Email);
            
            if (user == null || user.PasswordHash == null || 
                !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı");
                return View(model);
            }

            // Create claims
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new("IsGuest", user.IsGuest.ToString().ToLower())
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim("IsAdmin", "true"));
            }
            else
            {
                claims.Add(new Claim("IsAdmin", "false"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            // Redirect to return URL or home
            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Giriş yapılırken bir hata oluştu");
            return View(model);
        }
    }

    /// <summary>
    /// Register page
    /// </summary>
    [HttpGet]
    public IActionResult Register(
        string? email = null,
        string? firstName = null,
        string? lastName = null,
        string? identityNumber = null,
        string? phoneNumber = null,
        string? dateOfBirth = null,
        string? gender = null,
        string? fromBooking = null,
        string? pnr = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        ViewData["Title"] = "Kayıt Ol - Bulut Bilet.com";
        
        var model = new RegisterViewModel();
        
        // Pre-fill form if coming from booking
        if (!string.IsNullOrEmpty(fromBooking) && fromBooking.ToLower() == "true")
        {
            model.Email = email ?? "";
            model.FirstName = firstName ?? "";
            model.LastName = lastName ?? "";
            model.IdentityNumber = identityNumber ?? "";
            model.PhoneNumber = phoneNumber ?? "";
            model.Gender = gender ?? "Erkek";
            
            if (DateTime.TryParse(dateOfBirth, out var parsedDate))
            {
                model.DateOfBirth = parsedDate;
            }
            
            ViewBag.FromBooking = true;
            ViewBag.PNR = pnr;
            ViewBag.PreFilledMessage = "Rezervasyon bilgilerinizden form otomatik dolduruldu. Sadece şifrenizi belirleyin.";
        }
        
        return View(model);
    }

    /// <summary>
    /// Process registration
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ViewData["Title"] = "Kayıt Ol - Bulut Bilet.com";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(model.Email);
            if (existingUser != null && !existingUser.IsGuest)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor");
                return View(model);
            }

            // Create new user or convert guest user
            Core.Entities.User user;
            
            if (existingUser?.IsGuest == true)
            {
                // Convert guest user to registered user
                user = existingUser;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                user.PhoneNumber = model.PhoneNumber;
                user.IdentityNumber = model.IdentityNumber;
                user.DateOfBirth = model.DateOfBirth;
                user.Gender = model.Gender;
                user.IsGuest = false;
                user.UpdatedAt = DateTime.UtcNow;
                
                await _userRepository.UpdateAsync(user);
            }
            else
            {
                // Create new user
                user = new Core.Entities.User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    IdentityNumber = model.IdentityNumber,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    IsGuest = false,
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
            }

            _logger.LogInformation("User {Email} registered successfully", user.Email);

            // Auto login after registration
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FullName),
                new("IsGuest", "false")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            TempData["SuccessMessage"] = "Kayıt işleminiz başarıyla tamamlandı! Hoş geldiniz.";
            
            // Check if coming from booking flow
            var pnr = Request.Form["pnr"].ToString();
            if (!string.IsNullOrEmpty(pnr))
            {
                return RedirectToAction("Manage", "Booking", new { pnr = pnr, email = user.Email });
            }
            
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "Kayıt işlemi sırasında bir hata oluştu");
            return View(model);
        }
    }

    /// <summary>
    /// Guest login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GuestLogin(string email, string? returnUrl = null)
    {
        if (string.IsNullOrEmpty(email))
        {
            TempData["ErrorMessage"] = "E-posta adresi gereklidir";
            return RedirectToAction("Login");
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            
            if (user == null)
            {
                // Create new guest user
                user = new Core.Entities.User
                {
                    Email = email,
                    IsGuest = true,
                    IsAdmin = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
            }

            // Create claims for guest user
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.Email),
                new("IsGuest", "true")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            _logger.LogInformation("Guest user {Email} logged in", email);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Search", "Flights");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during guest login for {Email}", email);
            TempData["ErrorMessage"] = "Misafir girişi sırasında bir hata oluştu";
            return RedirectToAction("Login");
        }
    }

    /// <summary>
    /// Logout
    /// </summary>
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User logged out");
        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    /// Access denied page
    /// </summary>
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Erişim Engellendi - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Forgot password page
    /// </summary>
    [HttpGet]
    public IActionResult ForgotPassword()
    {
        ViewData["Title"] = "Şifremi Unuttum - Bulut Bilet.com";
        return View();
    }

    /// <summary>
    /// Process forgot password
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("", "E-posta adresi gereklidir.");
            return View();
        }

        try
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                // In a real application, you would send a password reset email here
                TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
                _logger.LogInformation("Password reset requested for email: {Email}", email);
            }
            else
            {
                // Don't reveal if email exists or not for security
                TempData["SuccessMessage"] = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            ModelState.AddModelError("", "Bir hata oluştu. Lütfen tekrar deneyin.");
            return View();
        }

        return View();
    }
}