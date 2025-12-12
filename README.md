# BULUTBİLET.COM - Uçak Bileti Rezervasyon Sistemi

Katmanlı mimari ile geliştirilmiş, modern ve profesyonel bir MVC Web uçak bileti rezervasyon sistemi.

## 🚀 Özellikler

### Kullanıcı Özellikleri
- ✅ Üye olmadan misafir girişi ile rezervasyon
- ✅ Kullanıcı kayıt ve giriş sistemi
- ✅ Akıllı uçuş arama ve benzer sefer önerileri
- ✅ Online bilet rezervasyonu
- ✅ **🪑 Görsel koltuk seçimi** - Cam kenarı/Koridor/Orta koltuk seçenekleri
- ✅ Bilet ödeme sistemi
- ✅ Uçuştan 24 saat öncesine kadar iptal/değişiklik
- ✅ Rezervasyon geçmişi görüntüleme
- ✅ PNR ile rezervasyon sorgulama
- ✅ E-posta ile rezervasyon arama

### Admin Özellikleri
- ✅ Uçuş ekleme, düzenleme, silme
- ✅ Tüm rezervasyonları görüntüleme
- ✅ Kullanıcı yönetimi
- ✅ Rezervasyon iptal etme
- ✅ Uçuş ve koltuk yönetimi

## 🏗️ Teknoloji Stack

### Backend
- **ASP.NET Core 9.0** - MVC Web Framework
- **Entity Framework Core** - ORM
- **SQLite** - Veritabanı
- **BCrypt** - Şifre hashleme
- **Font Awesome** - İkonlar

### Frontend
- **Razor Pages** - Server-side rendering
- **HTML5/CSS3** - Modern web standartları
- **JavaScript** - İnteraktif özellikler
- **Bootstrap** - Responsive tasarım

### Mimari
- **Katmanlı Mimari (Layered Architecture)**
  - Web Layer (MVC Controllers & Views)
  - Application Layer (DTOs, Mappings, Validators)
  - Core Layer (Entities, Interfaces)
  - Infrastructure Layer (Services)
  - Persistence Layer (Database, Repositories)

## 📦 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- SQLite (dahili - kurulum gerektirmez)

### Kurulum Adımları

1. Projeyi klonlayın
2. MVC Web projesini çalıştırın:
```bash
cd mvc-flight-booking/backend
dotnet run --project FlightBooking.Web --urls="http://localhost:5010"
```

Uygulama http://localhost:5010 adresinde çalışacaktır.

### Veritabanı
- SQLite veritabanı ilk çalıştırmada otomatik oluşturulur
- Test verileri otomatik yüklenir
- Herhangi bir manuel kurulum gerektirmez

## 👤 Test Kullanıcıları

### Admin Girişi
**E-posta:** admin@bulutbilet.com  
**Şifre:** Admin123!

### Test Kullanıcısı
**E-posta:** ahmet@test.com  
**Şifre:** Test123!

## 📁 Proje Yapısı

```
mvc-flight-booking/
└── backend/
    ├── FlightBooking.Web/          # MVC Web katmanı
    │   ├── Controllers/            # MVC Controllers & API Controllers
    │   ├── Views/                  # Razor Views
    │   ├── wwwroot/               # Static files (CSS, JS, images)
    │   ├── Program.cs             # Uygulama yapılandırması
    │   └── appsettings.json       # Ayarlar
    │
    ├── FlightBooking.Application/  # Uygulama katmanı
    │   ├── DTOs/                  # Data Transfer Objects
    │   ├── Mappings/              # AutoMapper profiles
    │   └── Validators/            # FluentValidation
    │
    ├── FlightBooking.Core/         # Domain katmanı
    │   ├── Entities/              # Domain modelleri
    │   ├── Interfaces/            # Repository interfaces
    │   └── Enums/                 # Enumerations
    │
    ├── FlightBooking.Infrastructure/ # Altyapı katmanı
    │   └── Services/              # Email, JWT vb. servisler
    │
    └── FlightBooking.Persistence/  # Veritabanı katmanı
        ├── Data/                  # DbContext, DbInitializer
        ├── Repositories/          # Repository implementations
        └── Migrations/            # EF Core migrations
```

## 🌐 Sayfalar ve Özellikler

### Ana Sayfalar
- `/` - Ana sayfa ve uçuş arama
- `/Flight/Search` - Uçuş arama ve listeleme
- `/Auth/Login` - Kullanıcı girişi
- `/Auth/Register` - Kullanıcı kaydı
- `/Booking/Query` - PNR ile rezervasyon sorgulama
- `/Booking/MyBookings` - Rezervasyonlarım

### Admin Sayfaları
- `/Admin` - Admin paneli
- `/Admin/Users` - Kullanıcı yönetimi

### API Endpoints (AJAX için)
- `GET /api/flights` - Uçuş listesi
- `POST /api/bookings` - Rezervasyon oluştur
- `GET /api/bookings/pnr/{pnr}` - PNR sorgulama
- `GET /api/bookings/email/{email}` - E-posta ile arama

## 🎨 Özellikler Detayı

### Misafir Kullanıcı
- Email adresi ile hızlı rezervasyon
- Üyelik gerektirmeden bilet satın alma

### Kayıtlı Kullanıcı
- Profil yönetimi
- Rezervasyon geçmişi
- Hızlı rezervasyon

### 24 Saat Kuralı
- Uçuştan 24 saat öncesine kadar iptal/değişiklik
- Otomatik koltuk iadesi
- Güvenli iptal süreci

### Responsive Tasarım
- Mobil uyumlu
- Modern ve kullanıcı dostu arayüz
- Gradient renkler ve animasyonlar

## 🛠️ Geliştirme

### Geliştirme Modu
```bash
cd mvc-flight-booking/backend
dotnet watch run --project FlightBooking.Web --urls="http://localhost:5010"
```

### Veritabanı Yönetimi
```bash
# Migration oluştur
dotnet ef migrations add MigrationName --project FlightBooking.Persistence --startup-project FlightBooking.Web

# Veritabanını güncelle
dotnet ef database update --project FlightBooking.Persistence --startup-project FlightBooking.Web

# Veritabanını sıfırla
dotnet ef database drop --force --project FlightBooking.Persistence --startup-project FlightBooking.Web
```

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Geliştirici

Expert seviyesinde katmanlı mimari ile geliştirilmiştir.
