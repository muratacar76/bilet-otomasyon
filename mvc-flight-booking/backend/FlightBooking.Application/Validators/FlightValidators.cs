using FluentValidation;
using FlightBooking.Application.DTOs;

namespace FlightBooking.Application.Validators;

/// <summary>
/// Flight request validator
/// </summary>
public class FlightRequestValidator : AbstractValidator<FlightRequest>
{
    public FlightRequestValidator()
    {
        RuleFor(x => x.FlightNumber)
            .NotEmpty().WithMessage("Uçuş numarası zorunludur")
            .Length(2, 10).WithMessage("Uçuş numarası 2-10 karakter arasında olmalıdır")
            .Matches(@"^[A-Z]{2}[0-9]+$").WithMessage("Uçuş numarası formatı: XX123 (2 harf + rakamlar)");

        RuleFor(x => x.Airline)
            .NotEmpty().WithMessage("Havayolu şirketi zorunludur")
            .Length(2, 50).WithMessage("Havayolu şirketi 2-50 karakter arasında olmalıdır");

        RuleFor(x => x.DepartureCity)
            .NotEmpty().WithMessage("Kalkış şehri zorunludur")
            .Length(2, 50).WithMessage("Kalkış şehri 2-50 karakter arasında olmalıdır");

        RuleFor(x => x.ArrivalCity)
            .NotEmpty().WithMessage("Varış şehri zorunludur")
            .Length(2, 50).WithMessage("Varış şehri 2-50 karakter arasında olmalıdır")
            .NotEqual(x => x.DepartureCity).WithMessage("Varış şehri kalkış şehrinden farklı olmalıdır");

        RuleFor(x => x.DepartureTime)
            .NotEmpty().WithMessage("Kalkış zamanı zorunludur")
            .GreaterThan(DateTime.Now).WithMessage("Kalkış zamanı gelecekte olmalıdır");

        RuleFor(x => x.ArrivalTime)
            .NotEmpty().WithMessage("Varış zamanı zorunludur")
            .GreaterThan(x => x.DepartureTime).WithMessage("Varış zamanı kalkış zamanından sonra olmalıdır");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır")
            .LessThan(50000).WithMessage("Fiyat 50.000 TL'den az olmalıdır");

        RuleFor(x => x.TotalSeats)
            .GreaterThan(0).WithMessage("Toplam koltuk sayısı 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(400).WithMessage("Toplam koltuk sayısı 400'den fazla olamaz");

        RuleFor(x => x.SeatsPerRow)
            .GreaterThan(0).WithMessage("Sıra başına koltuk sayısı 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(10).WithMessage("Sıra başına koltuk sayısı 10'dan fazla olamaz");

        RuleFor(x => x.TotalRows)
            .GreaterThan(0).WithMessage("Toplam sıra sayısı 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(80).WithMessage("Toplam sıra sayısı 80'den fazla olamaz");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Durum zorunludur")
            .Must(x => new[] { "Active", "Cancelled", "Delayed", "Completed" }.Contains(x))
            .WithMessage("Geçerli bir durum seçiniz");

        // Cross-field validation
        RuleFor(x => x)
            .Must(x => x.TotalSeats <= x.SeatsPerRow * x.TotalRows)
            .WithMessage("Toplam koltuk sayısı, sıra sayısı × sıra başına koltuk sayısından fazla olamaz");
    }
}

/// <summary>
/// Flight search request validator
/// </summary>
public class FlightSearchRequestValidator : AbstractValidator<FlightSearchRequest>
{
    public FlightSearchRequestValidator()
    {
        RuleFor(x => x.DepartureCity)
            .Length(2, 50).WithMessage("Kalkış şehri 2-50 karakter arasında olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.DepartureCity));

        RuleFor(x => x.ArrivalCity)
            .Length(2, 50).WithMessage("Varış şehri 2-50 karakter arasında olmalıdır")
            .NotEqual(x => x.DepartureCity).WithMessage("Varış şehri kalkış şehrinden farklı olmalıdır")
            .When(x => !string.IsNullOrEmpty(x.ArrivalCity) && !string.IsNullOrEmpty(x.DepartureCity));

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Kalkış tarihi bugünden önce olamaz")
            .LessThan(DateTime.Today.AddYears(1)).WithMessage("Kalkış tarihi 1 yıldan fazla ileri olamaz")
            .When(x => x.DepartureDate.HasValue);

        RuleFor(x => x.MinPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum fiyat 0'dan küçük olamaz")
            .When(x => x.MinPrice.HasValue);

        RuleFor(x => x.MaxPrice)
            .GreaterThan(x => x.MinPrice).WithMessage("Maksimum fiyat minimum fiyattan büyük olmalıdır")
            .When(x => x.MaxPrice.HasValue && x.MinPrice.HasValue);

        RuleFor(x => x.PassengerCount)
            .GreaterThan(0).WithMessage("Yolcu sayısı 0'dan büyük olmalıdır")
            .LessThanOrEqualTo(9).WithMessage("Yolcu sayısı 9'dan fazla olamaz");
    }
}