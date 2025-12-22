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
    /// Giriş sayfasını gösterir
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Kullanıcı zaten giriş yapmışsa ana sayfaya yönlendir
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        // Sayfa başlığını ve geri dönüş URL'ini ayarla
        ViewData["Title"] = "Giriş Yap - Bulut Bilet.com";
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Giriş işlemini gerçekleştirir
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["Title"] = "Giriş Yap - Bulut Bilet.com";

        // Model validasyonu başarısızsa formu tekrar göster
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Kullanıcıyı e-posta ile veritabanından bul
            var user = await _userRepository.GetByEmailAsync(model.Email);
            
            // Kullanıcı bulunamadıysa veya şifre yanlışsa hata ver
            if (user == null || user.PasswordHash == null || 
                !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı");
                return View(model);
            }

            // Kullanıcı bilgilerini claim'lere ekle (kimlik bilgileri)
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()), // Kullanıcı ID
                new(ClaimTypes.Email, user.Email), // E-posta
                new(ClaimTypes.Name, user.FullName), // Tam ad
                new("IsGuest", user.IsGuest.ToString().ToLower()) // Misafir mi?
            };

            // Admin kullanıcı ise admin claim'lerini ekle
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                claims.Add(new Claim("IsAdmin", "true"));
            }
            else
            {
                claims.Add(new Claim("IsAdmin", "false"));
            }

            // Kimlik bilgilerini oluştur
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Authentication özellikleri (cookie ayarları)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe, // "Beni hatırla" seçeneği
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8) // Cookie süresi
            };

            // Kullanıcıyı sisteme giriş yaptır (cookie oluştur)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            _logger.LogInformation("User {Email} logged in successfully", user.Email);

            // Geri dönüş URL'i varsa oraya, yoksa ana sayfaya yönlendir
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
            
            // Format phone number properly
            var phone = phoneNumber ?? "";
            if (!string.IsNullOrEmpty(phone))
            {
                // Remove all non-digits
                phone = new string(phone.Where(char.IsDigit).ToArray());
                
                // Add 0 prefix if not present
                if (phone.Length > 0 && phone[0] != '0')
                {
                    phone = "0" + phone;
                }
                
                // Format: 0555 123 45 67
                if (phone.Length == 11)
                {
                    phone = $"{phone.Substring(0, 4)} {phone.Substring(4, 3)} {phone.Substring(7, 2)} {phone.Substring(9, 2)}";
                }
            }
            model.PhoneNumber = phone;
            
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

        _logger.LogInformation("Register POST called for email: {Email}", model?.Email ?? "null");

        // Manual AcceptTerms validation
        if (!model.AcceptTerms)
        {
            ModelState.AddModelError("AcceptTerms", "Kullanım şartlarını kabul etmelisiniz");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Model validation failed for registration. Errors: {Errors}", 
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View(model);
        }

        try
        {
            _logger.LogInformation("Starting registration process for email: {Email}", model.Email);
            
            // Check if email already exists
            var existingUser = await _userRepository.GetByEmailAsync(model.Email);
            _logger.LogInformation("Existing user check result: {ExistingUser}", existingUser != null ? "Found" : "Not found");
            
            if (existingUser != null && !existingUser.IsGuest)
            {
                _logger.LogWarning("Email already exists for non-guest user: {Email}", model.Email);
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılıyor");
                return View(model);
            }

            // Create new user or convert guest user
            Core.Entities.User user;
            
            if (existingUser?.IsGuest == true)
            {
                _logger.LogInformation("Converting guest user to registered user: {Email}", model.Email);
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
                user.IsActive = true;
                user.UpdatedAt = DateTime.UtcNow;
                
                await _userRepository.UpdateAsync(user);
                _logger.LogInformation("Guest user converted successfully: {Email}", model.Email);
            }
            else
            {
                _logger.LogInformation("Creating new user: {Email}", model.Email);
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
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _userRepository.AddAsync(user);
                _logger.LogInformation("New user created successfully: {Email} with ID: {UserId}", model.Email, user.Id);
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