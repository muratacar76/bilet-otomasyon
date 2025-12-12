using AutoMapper;
using FlightBooking.Application.DTOs;
using FlightBooking.Core.Entities;

namespace FlightBooking.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity to DTO mappings
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()));
        
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.IsGuest, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IsAdmin, opt => opt.MapFrom(src => false));

        CreateMap<UpdateUserRequest, User>()
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // Flight mappings
        CreateMap<Flight, FlightDto>()
            .ForMember(dest => dest.Route, opt => opt.MapFrom(src => $"{src.DepartureCity} → {src.ArrivalCity}"))
            .ForMember(dest => dest.FlightDuration, opt => opt.MapFrom(src => src.ArrivalTime - src.DepartureTime))
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => src.Status == "Active" && src.AvailableSeats > 0))
            .ForMember(dest => dest.BookedSeats, opt => opt.MapFrom(src => src.TotalSeats - src.AvailableSeats));

        CreateMap<FlightRequest, Flight>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.TotalSeats));

        CreateMap<CreateFlightDto, Flight>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.TotalSeats))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
            .ForMember(dest => dest.SeatsPerRow, opt => opt.MapFrom(src => 6))
            .ForMember(dest => dest.TotalRows, opt => opt.MapFrom(src => src.TotalSeats / 6))
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.Seats, opt => opt.Ignore());

        CreateMap<UpdateFlightDto, Flight>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.TotalSeats))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"))
            .ForMember(dest => dest.SeatsPerRow, opt => opt.MapFrom(src => 6))
            .ForMember(dest => dest.TotalRows, opt => opt.MapFrom(src => src.TotalSeats / 6))
            .ForMember(dest => dest.Bookings, opt => opt.Ignore())
            .ForMember(dest => dest.Seats, opt => opt.Ignore());



        // Seat mappings
        CreateMap<Seat, SeatDto>()
            .ForMember(dest => dest.IsAvailable, opt => opt.MapFrom(src => !src.IsOccupied && !src.IsBlocked))
            .ForMember(dest => dest.SeatTypeDisplay, opt => opt.MapFrom(src => 
                src.SeatType == "Window" ? "🪟 Cam Kenarı" :
                src.SeatType == "Aisle" ? "🚶 Koridor" : "💺 Orta"));

        // Passenger mappings
        CreateMap<Passenger, PassengerDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}".Trim()))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => 
                DateTime.Now.Year - src.DateOfBirth.Year - 
                (DateTime.Now.DayOfYear < src.DateOfBirth.DayOfYear ? 1 : 0)))
            .ForMember(dest => dest.SeatTypeDisplay, opt => opt.MapFrom(src => 
                src.SeatType == "Window" ? "🪟 Cam Kenarı" :
                src.SeatType == "Aisle" ? "🚶 Koridor" : "💺 Orta"));

        CreateMap<PassengerDto, Passenger>();

        // Booking mappings
        CreateMap<Booking, BookingDto>()
            .ForMember(dest => dest.CanBeCancelled, opt => opt.MapFrom(src => 
                src.Status == "Confirmed" && src.Flight != null && 
                src.Flight.DepartureTime > DateTime.UtcNow.AddHours(24)))
            .ForMember(dest => dest.PricePerPassenger, opt => opt.MapFrom(src => 
                src.PassengerCount > 0 ? src.TotalPrice / src.PassengerCount : 0))
            .ForMember(dest => dest.StatusDisplay, opt => opt.MapFrom(src => 
                src.Status == "Confirmed" && src.IsPaid ? "Ödendi" :
                src.Status == "Confirmed" && !src.IsPaid ? "Onaylandı" :
                src.Status == "Cancelled" ? "İptal Edildi" :
                src.Status == "Pending" ? "Beklemede" : src.Status));

        CreateMap<BookingRequest, Booking>()
            .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Confirmed"))
            .ForMember(dest => dest.IsPaid, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.PassengerCount, opt => opt.MapFrom(src => src.Passengers.Count));

        // Booking summary mappings
        CreateMap<Booking, BookingSummaryDto>()
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : ""))
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.Flight != null ? src.Flight.FlightNumber : ""))
            .ForMember(dest => dest.Route, opt => opt.MapFrom(src => src.Flight != null ? $"{src.Flight.DepartureCity} → {src.Flight.ArrivalCity}" : ""))
            .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.Flight != null ? src.Flight.DepartureTime : DateTime.MinValue));
    }
}