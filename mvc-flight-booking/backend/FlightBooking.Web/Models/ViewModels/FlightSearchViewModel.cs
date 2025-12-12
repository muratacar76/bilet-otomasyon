using System.ComponentModel.DataAnnotations;
using FlightBooking.Core.Entities;

namespace FlightBooking.Web.Models.ViewModels
{
    public class FlightSearchViewModel
    {
        [Required(ErrorMessage = "Kalkış şehri seçiniz")]
        [Display(Name = "Nereden")]
        public string DepartureCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Varış şehri seçiniz")]
        [Display(Name = "Nereye")]
        public string ArrivalCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gidiş tarihi seçiniz")]
        [DataType(DataType.Date)]
        [Display(Name = "Gidiş Tarihi")]
        public DateTime DepartureDate { get; set; } = DateTime.Today;

        [Range(1, 10, ErrorMessage = "Yolcu sayısı 1-10 arasında olmalıdır")]
        [Display(Name = "Yolcu Sayısı")]
        public int PassengerCount { get; set; } = 1;

        [Display(Name = "Havayolu")]
        public string? Airline { get; set; }

        [Display(Name = "Maksimum Fiyat")]
        [Range(0, double.MaxValue, ErrorMessage = "Geçerli bir fiyat girin")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Sıralama")]
        public string SortBy { get; set; } = "Price";

        // Search results
        public List<Core.Entities.Flight> Flights { get; set; } = new List<Core.Entities.Flight>();
        
        // Search metadata
        public int TotalResults { get; set; }
        public bool HasSearched { get; set; }
        public string? SearchMessage { get; set; }
    }
}