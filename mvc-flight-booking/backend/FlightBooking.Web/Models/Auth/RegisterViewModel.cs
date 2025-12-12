using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Web.Models.Auth;

/// <summary>
/// Register view model
/// </summary>
public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad zorunludur")]
    [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur")]
    [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur")]
    [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Şifre Tekrar")]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur")]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "TC Kimlik numarası zorunludur")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "TC Kimlik numarası 11 haneli olmalıdır")]
    [Display(Name = "TC Kimlik No")]
    public string IdentityNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi zorunludur")]
    [DataType(DataType.Date)]
    [Display(Name = "Doğum Tarihi")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Cinsiyet seçimi zorunludur")]
    [Display(Name = "Cinsiyet")]
    public string Gender { get; set; } = string.Empty;

    [Display(Name = "Kullanım şartlarını kabul ediyorum")]
    public bool AcceptTerms { get; set; }
}