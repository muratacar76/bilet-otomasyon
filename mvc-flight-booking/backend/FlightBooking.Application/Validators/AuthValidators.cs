using FluentValidation;
using FlightBooking.Application.DTOs;

namespace FlightBooking.Application.Validators;

/// <summary>
/// Register request validator
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad alanı zorunludur")
            .Length(2, 50).WithMessage("Ad 2-50 karakter arasında olmalıdır");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad alanı zorunludur")
            .Length(2, 50).WithMessage("Soyad 2-50 karakter arasında olmalıdır");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı zorunludur")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur")
            .Matches(@"^0[0-9]{10}$").WithMessage("Geçerli bir telefon numarası giriniz (0XXXXXXXXXX)");

        RuleFor(x => x.IdentityNumber)
            .NotEmpty().WithMessage("TC Kimlik numarası zorunludur")
            .Length(11).WithMessage("TC Kimlik numarası 11 haneli olmalıdır")
            .Matches(@"^[0-9]+$").WithMessage("TC Kimlik numarası sadece rakam içermelidir")
            .Must(BeValidTCKimlik).WithMessage("Geçersiz TC Kimlik numarası");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Doğum tarihi zorunludur")
            .LessThan(DateTime.Now.AddYears(-18)).WithMessage("18 yaşından küçük olamazsınız")
            .GreaterThan(DateTime.Now.AddYears(-100)).WithMessage("Geçerli bir doğum tarihi giriniz");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Cinsiyet seçimi zorunludur")
            .Must(x => x == "Erkek" || x == "Kadın").WithMessage("Geçerli bir cinsiyet seçiniz");
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
/// Login request validator
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre alanı zorunludur");
    }
}

/// <summary>
/// Guest login request validator
/// </summary>
public class GuestLoginRequestValidator : AbstractValidator<GuestLoginRequest>
{
    public GuestLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta alanı zorunludur")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz");
    }
}

/// <summary>
/// Change password request validator
/// </summary>
public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifre zorunludur");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre zorunludur")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir");
    }
}