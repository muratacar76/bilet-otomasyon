using FluentValidation;
using FlightBooking.Application.DTOs;

namespace FlightBooking.Application.Validators;

/// <summary>
/// Booking request validator
/// </summary>
public class BookingRequestValidator : AbstractValidator<BookingRequest>
{
    public BookingRequestValidator()
    {
        RuleFor(x => x.FlightId)
            .GreaterThan(0).WithMessage("Geçerli bir uçuş seçiniz");

        RuleFor(x => x.Passengers)
            .NotEmpty().WithMessage("En az bir yolcu bilgisi girilmelidir")
            .Must(x => x.Count <= 9).WithMessage("En fazla 9 yolcu için rezervasyon yapabilirsiniz");

        RuleForEach(x => x.Passengers)
            .SetValidator(new PassengerDtoValidator());

        RuleFor(x => x.GuestEmail)
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
            .When(x => !string.IsNullOrEmpty(x.GuestEmail));

        // Ensure no duplicate TC numbers
        RuleFor(x => x.Passengers)
            .Must(passengers => passengers.Select(p => p.IdentityNumber).Distinct().Count() == passengers.Count)
            .WithMessage("Aynı TC Kimlik numarası birden fazla yolcu için kullanılamaz");
    }
}

/// <summary>
/// Passenger DTO validator
/// </summary>
public class PassengerDtoValidator : AbstractValidator<PassengerDto>
{
    public PassengerDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Yolcu adı zorunludur")
            .Length(2, 50).WithMessage("Yolcu adı 2-50 karakter arasında olmalıdır");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Yolcu soyadı zorunludur")
            .Length(2, 50).WithMessage("Yolcu soyadı 2-50 karakter arasında olmalıdır");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("TC Kimlik numarası zorunludur")
            .Length(11).WithMessage("TC Kimlik numarası 11 haneli olmalıdır")
            .Matches(@"^[0-9]+$").WithMessage("TC Kimlik numarası sadece rakam içermelidir")
            .Must(BeValidTCKimlik).WithMessage("Geçersiz TC Kimlik numarası");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Doğum tarihi zorunludur")
            .LessThan(DateTime.Now).WithMessage("Doğum tarihi gelecekte olamaz")
            .GreaterThan(DateTime.Now.AddYears(-120)).WithMessage("Geçerli bir doğum tarihi giriniz");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Cinsiyet seçimi zorunludur")
            .Must(x => x == "Erkek" || x == "Kadın").WithMessage("Geçerli bir cinsiyet seçiniz");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur")
            .Matches(@"^0[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz (0XXXXXXXXXX)");

        RuleFor(x => x.SeatNumber)
            .Matches(@"^[0-9]{1,2}[A-F]$").WithMessage("Geçerli bir koltuk numarası giriniz (örn: 12A)")
            .When(x => !string.IsNullOrEmpty(x.SeatNumber));

        RuleFor(x => x.SeatType)
            .Must(x => new[] { "Window", "Aisle", "Middle" }.Contains(x))
            .WithMessage("Geçerli bir koltuk tipi seçiniz")
            .When(x => !string.IsNullOrEmpty(x.SeatType));
    }

    private bool BeValidTCKimlik(string tcKimlik)
    {
        if (string.IsNullOrEmpty(tcKimlik) || tcKimlik.Length != 11)
            return false;

        if (!tcKimlik.All(char.IsDigit))
            return false;

        if (tcKimlik[0] == '0')
            return false;

        if (tcKimlik.All(c => c == tcKimlik[0]))
            return false;

        var digits = tcKimlik.Select(c => int.Parse(c.ToString())).ToArray();

        var sum = digits.Take(10).Sum();
        if (sum % 10 != digits[10])
            return false;

        var oddSum = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
        var evenSum = digits[1] + digits[3] + digits[5] + digits[7];
        if ((oddSum * 7 - evenSum) % 10 != digits[9])
            return false;

        return true;
    }
}

/// <summary>
/// Payment request validator
/// </summary>
public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("Geçerli bir rezervasyon seçiniz");

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Ödeme yöntemi seçiniz")
            .Must(x => new[] { "CreditCard", "BankTransfer", "Cash" }.Contains(x))
            .WithMessage("Geçerli bir ödeme yöntemi seçiniz");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Ödeme tutarı 0'dan büyük olmalıdır");

        // Credit card validations
        When(x => x.PaymentMethod == "CreditCard", () =>
        {
            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("Kart numarası zorunludur")
                .Matches(@"^[0-9]{16}$").WithMessage("Kart numarası 16 haneli olmalıdır");

            RuleFor(x => x.CardHolderName)
                .NotEmpty().WithMessage("Kart sahibi adı zorunludur")
                .Length(2, 50).WithMessage("Kart sahibi adı 2-50 karakter arasında olmalıdır");

            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Son kullanma tarihi zorunludur")
                .Matches(@"^(0[1-9]|1[0-2])\/[0-9]{2}$").WithMessage("Son kullanma tarihi MM/YY formatında olmalıdır");

            RuleFor(x => x.CVV)
                .NotEmpty().WithMessage("CVV kodu zorunludur")
                .Matches(@"^[0-9]{3,4}$").WithMessage("CVV kodu 3-4 haneli olmalıdır");
        });
    }
}

/// <summary>
/// PNR query request validator
/// </summary>
public class PNRQueryRequestValidator : AbstractValidator<PNRQueryRequest>
{
    public PNRQueryRequestValidator()
    {
        RuleFor(x => x.PNR)
            .NotEmpty().WithMessage("PNR numarası zorunludur")
            .Length(6).WithMessage("PNR numarası 6 karakterli olmalıdır")
            .Matches(@"^[A-Z0-9]+$").WithMessage("PNR numarası sadece büyük harf ve rakam içermelidir");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");
    }
}