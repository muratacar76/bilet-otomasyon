# Bulut Bilet.com - MVC Uçak Bileti Rezervasyon Sistemi

Klasik MVC mimarisine uygun olarak geliştirilmiş, kapsamlı uçak bileti rezervasyon sistemi.

## 🚀 Özellikler

### Kullanıcı Özellikleri
- ✅ Üye olmadan misafir girişi ile rezervasyon
- ✅ Kullanıcı kayıt ve giriş sistemi
- ✅ Uçuş arama ve filtreleme
- ✅ Online bilet rezervasyonu
- ✅ **🪑 Görsel koltuk seçimi** - Cam kenarı/Koridor/Orta koltuk seçenekleri
- ✅ Bilet ödeme sistemi
- ✅ Uçuştan 24 saat öncesine kadar iptal/değişiklik
- ✅ Rezervasyon geçmişi görüntüleme
- ✅ PNR ile rezervasyon sorgulama

### Admin Özellikleri
- ✅ Uçuş ekleme, düzenleme, silme
- ✅ Tüm rezervasyonları görüntüleme
- ✅ Rezervasyon iptal etme
- ✅ Kullanıcı yönetimi
- ✅ Uçuş ve koltuk yönetimi

## 🏗️ Teknoloji Stack

### Backend & Frontend (ASP.NET Core MVC)
- **ASP.NET Core 8.0** - Web Framework
- **Entity Framework Core** - ORM
- **SQLite** - Veritabanı
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - CSS Framework
- **jQuery** - JavaScript library
- **SignalR** - Real-time communication
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **BCrypt** - Şifre hashleme

### Mimari
- **MVC (Model-View-Controller) Pattern**
  - **Models**: Entity models, ViewModels, DTOs
  - **Views**: Razor views (.cshtml), Partial views
  - **Controllers**: MVC Controllers, Action methods
  - **Services**: Business logic layer
  - **Repositories**: Data access layer

## 📦 Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQLite (dahili - kurulum gerektirmez)

### Uygulama Kurulumu

1. Proje klasörüne gidin:
```bash
cd mvc-flight-booking/backend
```

2. Bağımlılıkları yükleyin:
```bash
dotnet restore
```

3. Veritabanını oluşturun:
```bash
dotnet ef database update --project FlightBooking.Persistence --startup-project FlightBooking.Web
```

4. Uygulamayı çalıştırın:
```bash
dotnet run --project FlightBooking.Web
```

Uygulama http://localhost:5000 adresinde çalışacaktır.

## 👤 Admin Girişi

**Kullanıcı Adı:** admin@bulutbilet.com  
**Şifre:** Admin123!

## 📁 Proje Yapısı

```
mvc-flight-booking/
└── backend/                           # ASP.NET Core MVC Application
    ├── FlightBooking.Web/            # MVC Web Application
    │   ├── Controllers/              # MVC Controllers
    │   │   ├── HomeController.cs     # Ana sayfa
    │   │   ├── AuthController.cs     # Kimlik doğrulama
    │   │   ├── FlightsController.cs  # Uçuş yönetimi
    │   │   ├── BookingsController.cs # Rezervasyon yönetimi
    │   │   └── AdminController.cs    # Admin paneli
    │   │
    │   ├── Views/                    # Razor Views
    │   │   ├── Shared/               # Layout ve ortak views
    │   │   │   ├── _Layout.cshtml    # Ana layout
    │   │   │   ├── _AdminLayout.cshtml # Admin layout
    │   │   │   └── _LoginPartial.cshtml # Login partial
    │   │   ├── Home/                 # Ana sayfa views
    │   │   ├── Auth/                 # Kimlik doğrulama views
    │   │   ├── Flights/              # Uçuş views
    │   │   ├── Bookings/             # Rezervasyon views
    │   │   └── Admin/                # Admin views
    │   │
    │   ├── Models/                   # ViewModels
    │   │   ├── Auth/                 # Authentication ViewModels
    │   │   ├── Flight/               # Flight ViewModels
    │   │   ├── Booking/              # Booking ViewModels
    │   │   └── Admin/                # Admin ViewModels
    │   │
    │   ├── wwwroot/                  # Static files
    │   │   ├── css/                  # CSS files
    │   │   ├── js/                   # JavaScript files
    │   │   ├── images/               # Images
    │   │   └── lib/                  # Third-party libraries
    │   │
    │   ├── Services/                 # Application services
    │   └── Program.cs                # Application entry point
    │
    ├── FlightBooking.Core/           # Domain Layer (Models)
    │   ├── Entities/                 # Domain entities
    │   ├── Interfaces/               # Repository interfaces
    │   └── Enums/                    # Enumerations
    │
    ├── FlightBooking.Application/    # Application Layer (Services)
    │   ├── Services/                 # Business logic services
    │   ├── DTOs/                     # Data Transfer Objects
    │   ├── Validators/               # Input validators
    │   └── Mappings/                 # AutoMapper profiles
    │
    ├── FlightBooking.Infrastructure/ # Infrastructure Layer
    │   ├── Services/                 # External services (JWT, Email)
    │   └── Configurations/           # Service configurations
    │
    └── FlightBooking.Persistence/    # Data Access Layer
        ├── Repositories/             # Repository implementations
        ├── Data/                     # DbContext and configurations
        └── Migrations/               # EF Core migrations
```

## 🌐 MVC Routes

### Public Routes
- `GET /` - Ana sayfa
- `GET /Auth/Login` - Giriş sayfası
- `GET /Auth/Register` - Kayıt sayfası
- `GET /Flights` - Uçuş arama
- `GET /Flights/Search` - Uçuş sonuçları
- `GET /Bookings/PNR` - PNR sorgulama

### Protected Routes
- `GET /Bookings` - Rezervasyonlarım
- `GET /Bookings/Details/{id}` - Rezervasyon detayı
- `GET /Auth/Profile` - Profil sayfası

### Admin Routes
- `GET /Admin` - Admin dashboard
- `GET /Admin/Flights` - Uçuş yönetimi
- `GET /Admin/Bookings` - Rezervasyon yönetimi
- `GET /Admin/Users` - Kullanıcı yönetimi

## 🎨 MVC Mimari Detayları

### Models (Data Layer)
- **Entity Models**: User, Flight, Booking, Passenger, Seat
- **ViewModels**: Razor view'lar için özel modeller
- **DTOs**: Veri transferi için kullanılan modeller
- **Validation Models**: Form doğrulama modelleri

### Views (Presentation Layer)
- **Razor Views**: Server-side rendering ile HTML üretimi
- **Layout Pages**: Tutarlı sayfa yapısı için
- **Partial Views**: Tekrar kullanılabilir bileşenler
- **ViewComponents**: Karmaşık UI bileşenleri

### Controllers (Logic Layer)
- **MVC Controllers**: HTTP isteklerini karşılama
- **Action Methods**: Kullanıcı etkileşimlerini işleme
- **Filters**: Cross-cutting concerns (authentication, logging)
- **Model Binding**: Form verilerini modellere bağlama

## 🔧 Geliştirme

### Uygulamayı Çalıştırma
```bash
cd backend
dotnet watch run --project FlightBooking.Web
```

### Veritabanı Migration
```bash
cd backend
dotnet ef migrations add MigrationName --project FlightBooking.Persistence --startup-project FlightBooking.Web
dotnet ef database update --project FlightBooking.Persistence --startup-project FlightBooking.Web
```

### CSS/JS Değişiklikleri
```bash
# wwwroot klasöründeki dosyalar otomatik olarak güncellenir
# Tarayıcıda Ctrl+F5 ile cache'i temizleyerek yenileyin
```

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Integration Tests
```bash
cd backend
dotnet test --filter Category=Integration
```

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Geliştirici

Expert seviyesinde ASP.NET Core MVC mimarisi ile geliştirilmiştir.