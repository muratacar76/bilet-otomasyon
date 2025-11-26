# FlightBooking.Persistence Katmanı

Bu katman, veritabanı erişim mantığını ve Entity Framework Core yapılandırmalarını içerir.

## 📁 Klasör Yapısı

```
FlightBooking.Persistence/
├── Data/
│   ├── ApplicationDbContext.cs      # Ana DbContext
│   ├── DbInitializer.cs            # Seed data
│   ├── Repositories/               # Repository pattern
│   │   ├── IRepository.cs          # Generic repository interface
│   │   └── Repository.cs           # Generic repository implementation
│   └── UnitOfWork/                 # Unit of Work pattern
│       ├── IUnitOfWork.cs          # UnitOfWork interface
│       └── UnitOfWork.cs           # UnitOfWork implementation
│
├── Models/                         # Entity configurations
│   ├── UserConfiguration.cs        # User entity yapılandırması
│   ├── FlightConfiguration.cs      # Flight entity yapılandırması
│   ├── BookingConfiguration.cs     # Booking entity yapılandırması
│   └── PassengerConfiguration.cs   # Passenger entity yapılandırması
│
└── Migrations/                     # EF Core migrations (otomatik oluşur)
```

## 🎯 Katman Sorumlulukları

### Data Klasörü
- **ApplicationDbContext**: Entity Framework Core DbContext
- **DbInitializer**: Başlangıç verilerini (seed data) yükler
- **Repositories**: Generic repository pattern implementasyonu
- **UnitOfWork**: Transaction yönetimi ve repository koordinasyonu

### Models Klasörü
- Her entity için ayrı configuration sınıfı
- Fluent API ile entity yapılandırmaları
- İlişki tanımlamaları (Foreign Keys, Indexes)
- Validasyon kuralları
- Seed data tanımlamaları

### Migrations Klasörü
- Entity Framework Core migration dosyaları
- Veritabanı şema değişiklikleri
- Otomatik olarak oluşturulur

## 🔧 Kullanım

### Repository Pattern Kullanımı

```csharp
// Controller'da kullanım
public class FlightsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public FlightsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> GetFlights()
    {
        var flights = await _unitOfWork.Flights.GetAllAsync();
        return Ok(flights);
    }
}
```

### Transaction Kullanımı

```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    await _unitOfWork.Bookings.AddAsync(booking);
    await _unitOfWork.Passengers.AddRangeAsync(passengers);
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

## 📝 Entity Configuration Örneği

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);
        
        // Properties
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        // Indexes
        builder.HasIndex(e => e.Email)
            .IsUnique();
            
        // Seed Data
        builder.HasData(new User { ... });
    }
}
```

## 🚀 Migration Komutları

```bash
# Yeni migration oluştur
dotnet ef migrations add MigrationName --project src/FlightBooking.Persistence --startup-project src/FlightBooking.API

# Veritabanını güncelle
dotnet ef database update --project src/FlightBooking.Persistence --startup-project src/FlightBooking.API

# Migration'ı geri al
dotnet ef migrations remove --project src/FlightBooking.Persistence --startup-project src/FlightBooking.API

# Migration listesi
dotnet ef migrations list --project src/FlightBooking.Persistence --startup-project src/FlightBooking.API
```

## 🎨 Design Patterns

### Repository Pattern
- Generic repository ile kod tekrarını önler
- CRUD operasyonlarını merkezi bir yerden yönetir
- Test edilebilirliği artırır

### Unit of Work Pattern
- Transaction yönetimi sağlar
- Birden fazla repository'yi koordine eder
- Atomik işlemler için kullanılır

### Configuration Pattern
- Entity yapılandırmalarını DbContext'ten ayırır
- Her entity için ayrı configuration sınıfı
- Fluent API kullanımı
- Daha temiz ve okunabilir kod

## 📊 Veritabanı Şeması

### Users
- Kullanıcı bilgileri
- Admin ve misafir kullanıcı desteği
- Email unique constraint

### Flights
- Uçuş bilgileri
- Koltuk yönetimi
- Durum takibi (Active/Cancelled)

### Bookings
- Rezervasyon bilgileri
- Ödeme durumu
- User ve Flight ile ilişkili

### Passengers
- Yolcu bilgileri
- Booking ile ilişkili
- Cascade delete

## 🔐 Güvenlik

- SQL Injection koruması (EF Core parametreli sorgular)
- Transaction desteği ile veri tutarlılığı
- Foreign key constraints
- Index optimizasyonları

## 📈 Performans

- Asenkron operasyonlar
- Index kullanımı
- Lazy loading devre dışı
- Explicit loading ile gerektiğinde ilişkili veri yükleme

## 🎯 Best Practices

1. ✅ Her entity için ayrı configuration
2. ✅ Generic repository kullanımı
3. ✅ Unit of Work ile transaction yönetimi
4. ✅ Asenkron operasyonlar
5. ✅ Proper indexing
6. ✅ Foreign key constraints
7. ✅ Seed data ile test verisi
8. ✅ Migration ile version control
