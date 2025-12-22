// Gerekli namespace'leri içe aktarıyoruz
using FlightBooking.Application.Services;
using FlightBooking.Application.Validators;
using FlightBooking.Core.Interfaces;
using FlightBooking.Infrastructure.Services;
using FlightBooking.Persistence.Data;
using FlightBooking.Persistence.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Web uygulaması builder'ını oluşturuyoruz
var builder = WebApplication.CreateBuilder(args);

// Serilog yapılandırması - Loglama için kullanılıyor
// Console'a ve günlük dosyalara log yazıyor
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Konsola log yaz
    .WriteTo.File("logs/bulutbilet-.txt", rollingInterval: RollingInterval.Day) // Günlük dosyalara log yaz
    .CreateLogger();

// Serilog'u host'a entegre ediyoruz
builder.Host.UseSerilog();

// Servisleri container'a ekliyoruz
// MVC Controller'ları ve View'ları kullanabilmek için
builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation(); // Geliştirme için - View'ları anında yeniden derler (hot reload)

// Veritabanı bağlantısını yapılandırıyoruz
// SQLite veritabanı kullanıyoruz
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                     "Data Source=BulutBilet.db", o => o.CommandTimeout(30))); // 30 saniye timeout

// Kimlik doğrulama (Authentication) yapılandırması
// Cookie tabanlı kimlik doğrulama kullanıyoruz
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Giriş sayfası yolu
        options.LogoutPath = "/Auth/Logout"; // Çıkış sayfası yolu
        options.AccessDeniedPath = "/Auth/AccessDenied"; // Erişim reddedildi sayfası
        options.ExpireTimeSpan = TimeSpan.FromDays(1); // Cookie 1 gün geçerli
        options.SlidingExpiration = true; // Her istekte süre yenilenir
        options.Cookie.Name = "BulutBilet.Auth"; // Cookie adı
        options.Cookie.HttpOnly = true; // JavaScript'ten erişilemez (güvenlik)
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS kontrolü
    });

// Yetkilendirme (Authorization) yapılandırması
// Admin rolü için özel policy tanımlıyoruz
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")); // Sadece Admin rolü erişebilir
});

// AutoMapper - DTO ve Entity dönüşümleri için kullanılıyor
builder.Services.AddAutoMapper(typeof(FlightBooking.Application.Mappings.MappingProfile));

// FluentValidation - Form validasyonları için kullanılıyor
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// Repository'leri Dependency Injection container'a ekliyoruz
// Scoped: Her HTTP isteği için yeni bir instance oluşturulur
builder.Services.AddScoped<IUserRepository, UserRepository>(); // Kullanıcı işlemleri
builder.Services.AddScoped<IFlightRepository, FlightRepository>(); // Uçuş işlemleri
builder.Services.AddScoped<IBookingRepository, BookingRepository>(); // Rezervasyon işlemleri
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>(); // Yolcu işlemleri
builder.Services.AddScoped<ISeatRepository, SeatRepository>(); // Koltuk işlemleri

// Uygulama servislerini ekliyoruz
builder.Services.AddScoped<IEmailService, EmailService>(); // E-posta gönderme servisi
builder.Services.AddScoped<IJwtService>(provider => // JWT token oluşturma servisi
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    // JWT ayarlarını configuration'dan alıyoruz, yoksa varsayılan değerleri kullanıyoruz
    var secretKey = configuration["JwtSettings:SecretKey"] ?? "BulutBiletSuperSecretKeyForJWTTokenGeneration12345678901234567890";
    var issuer = configuration["JwtSettings:Issuer"] ?? "BulutBiletWeb";
    var audience = configuration["JwtSettings:Audience"] ?? "BulutBiletClient";
    return new JwtService(secretKey, issuer, audience);
});

// Session yapılandırması - Kullanıcı oturumu için
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dakika işlem yapılmazsa session sona erer
    options.Cookie.HttpOnly = true; // JavaScript'ten erişilemez (güvenlik)
    options.Cookie.IsEssential = true; // GDPR için gerekli cookie
    options.Cookie.Name = "BulutBilet.Session"; // Session cookie adı
});

// HttpContext'e servislerden erişebilmek için HttpContextAccessor ekliyoruz
builder.Services.AddHttpContextAccessor();

// Uygulamayı build ediyoruz
var app = builder.Build();

// HTTP request pipeline'ını yapılandırıyoruz
// Production ortamında hata sayfası ve HSTS kullanıyoruz
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Hata sayfasına yönlendir
    app.UseHsts(); // HTTP Strict Transport Security - HTTPS zorunluluğu
}

app.UseHttpsRedirection(); // HTTP isteklerini HTTPS'e yönlendir
app.UseStaticFiles(); // wwwroot klasöründeki statik dosyaları servis et (CSS, JS, resimler)

app.UseRouting(); // Route'ları etkinleştir
app.UseSession(); // Session'ı etkinleştir

app.UseAuthentication(); // Kimlik doğrulamayı etkinleştir
app.UseAuthorization(); // Yetkilendirmeyi etkinleştir

// Route'ları yapılandırıyoruz
// Admin paneli için özel route
app.MapControllerRoute(
    name: "admin",
    pattern: "Admin/{action=Index}/{id?}", // /Admin/Users gibi URL'ler
    defaults: new { controller = "Admin" }); // Varsayılan controller: Admin

// Varsayılan MVC route pattern'i
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"); // /Controller/Action/Id formatı

// API controller'ları için route mapping
app.MapControllers();

// Veritabanını başlangıç verileriyle dolduruyoruz (Seed)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await DbInitializer.InitializeAsync(context); // Test verileri ekleniyor
        Log.Information("Database initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while initializing the database");
    }
}

Log.Information("Bulut Bilet.com MVC Application starting up...");

// Uygulamayı başlat
app.Run();

public partial class Program { } // Test projesi için gerekli