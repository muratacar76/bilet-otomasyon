using System.ComponentModel.DataAnnotations;

namespace FlightBooking.Web.Models.Flight;

/// <summary>
/// Flight search view model
/// </summary>
public class FlightSearchViewModel
{
    [Display(Name = "Kalkış Şehri")]
    public string? DepartureCity { get; set; }

    [Display(Name = "Varış Şehri")]
    public string? ArrivalCity { get; set; }

    [Display(Name = "Kalkış Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? DepartureDate { get; set; }

    [Display(Name = "Minimum Fiyat")]
    [Range(0, 50000, ErrorMessage = "Fiyat 0-50000 TL arasında olmalıdır")]
    public decimal? MinPrice { get; set; }

    [Display(Name = "Maksimum Fiyat")]
    [Range(0, 50000, ErrorMessage = "Fiyat 0-50000 TL arasında olmalıdır")]
    public decimal? MaxPrice { get; set; }

    [Display(Name = "Yolcu Sayısı")]
    [Range(1, 9, ErrorMessage = "Yolcu sayısı 1-9 arasında olmalıdır")]
    public int PassengerCount { get; set; } = 1;

    // Results
    public List<FlightViewModel> Results { get; set; } = new();
    public bool HasSearched { get; set; }
    public string? SearchMessage { get; set; }
}