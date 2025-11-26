# Uçak Bileti Rezervasyon Sistemi

Katmanlı mimari ile geliştirilmiş, modern ve profesyonel bir uçak bileti rezervasyon sistemi.

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

### Admin Özellikleri
- ✅ Uçuş ekleme, düzenleme, silme
- ✅ Tüm rezervasyonları görüntüleme
- ✅ Rezervasyon iptal etme
- ✅ Uçuş ve koltuk yönetimi

## 🏗️ Teknoloji Stack

### Backend
- **ASP.NET Core 9.0** - Web API
- **Entity Framework Core** - ORM
- **SQL Server** - Veritabanı
- **JWT** - Authentication
- **BCrypt** - Şifre hashleme

### Frontend
- **React 19** - UI Framework
- **React Router** - Routing
- **Axios** - HTTP Client
- **Vite** - Build Tool

### Mimari
- **Katmanlı Mimari (Layered Architecture)**
  - API Layer
  - Application Layer
  - Core Layer (Entities)
  - Infrastructure Layer (Services)
  - Persistence Layer (Database)

## 📦 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Node.js 20+
- SQL Server (LocalDB)

### Backend Kurulumu

1. Projeyi klonlayın
2. Veritabanını oluşturun:
```bash
# Not: dotnet-ef tool kurulu değilse önce kurun
dotnet tool install --global dotnet-ef

# Migration oluştur ve veritabanını güncelle
dotnet ef database update --project src/FlightBooking.Persistence --startup-project src/FlightBooking.API
```

3. Backend'i çalıştırın:
```bash
dotnet run --project src/FlightBooking.API
```

Backend http://localhost:5000 adresinde çalışacaktır.

### Frontend Kurulumu

1. Client klasörüne gidin:
```bash
cd client
```

2. Bağımlılıkları yükleyin (zaten yüklü):
```bash
npm install
```

3. Frontend'i çalıştırın:
```bash
npm run dev
```

Frontend http://localhost:3000 adresinde çalışacaktır.

## 👤 Admin Girişi

**Kullanıcı Adı:** admin@flightbooking.com  
**Şifre:** 1234

## 📁 Proje Yapısı

```
FlightBookingSystem/
├── src/
│   ├── FlightBooking.API/          # Web API katmanı
│   │   ├── Controllers/            # API Controllers
│   │   ├── Program.cs              # Uygulama yapılandırması
│   │   └── appsettings.json        # Ayarlar
│   │
│   ├── FlightBooking.Application/  # Uygulama katmanı
│   │   └── DTOs/                   # Data Transfer Objects
│   │
│   ├── FlightBooking.Core/         # Domain katmanı
│   │   └── Entities/               # Domain modelleri
│   │
│   ├── FlightBooking.Infrastructure/ # Altyapı katmanı
│   │   └── Services/               # JWT, Email vb. servisler
│   │
│   └── FlightBooking.Persistence/  # Veritabanı katmanı
│       └── Data/                   # DbContext, Repositories
│
└── client/                         # React Frontend
    ├── src/
    │   ├── pages/                  # Sayfa bileşenleri
    │   ├── App.jsx                 # Ana uygulama
    │   └── main.jsx                # Giriş noktası
    └── index.html
```

## 🔐 API Endpoints

### Authentication
- `POST /api/auth/register` - Kullanıcı kaydı
- `POST /api/auth/login` - Kullanıcı girişi
- `POST /api/auth/guest` - Misafir girişi

### Flights
- `GET /api/flights` - Uçuşları listele/ara
- `GET /api/flights/{id}` - Uçuş detayı
- `POST /api/flights` - Uçuş ekle (Admin)
- `PUT /api/flights/{id}` - Uçuş güncelle (Admin)
- `DELETE /api/flights/{id}` - Uçuş sil (Admin)

### Bookings
- `GET /api/bookings` - Kullanıcının rezervasyonları
- `GET /api/bookings/{id}` - Rezervasyon detayı
- `POST /api/bookings` - Rezervasyon oluştur
- `POST /api/bookings/{id}/pay` - Ödeme yap
- `DELETE /api/bookings/{id}` - Rezervasyon iptal
- `GET /api/bookings/all` - Tüm rezervasyonlar (Admin)

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

### Backend Geliştirme
```bash
dotnet watch run --project src/FlightBooking.API
```

### Frontend Geliştirme
```bash
cd client
npm run dev
```

## 📝 Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## 👨‍💻 Geliştirici

Expert seviyesinde katmanlı mimari ile geliştirilmiştir.
