using FlightBooking.Application.DTOs;

namespace FlightBooking.Application.Services;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GuestLoginAsync(GuestLoginRequest request);
    Task<AuthResponse> RefreshTokenAsync(string token);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UserDto> UpdateUserAsync(int userId, UpdateUserRequest request);
    Task<UserDto?> GetUserByIdAsync(int userId);
    Task<UserDto?> GetUserByEmailAsync(string email);
    Task<bool> ValidateTokenAsync(string token);
    Task LogoutAsync(string token);
}

/// <summary>
/// JWT service interface
/// </summary>
public interface IJwtService
{
    string GenerateToken(Core.Entities.User user);
    string? ValidateToken(string token);
    DateTime GetTokenExpiration(string token);
    bool IsTokenExpired(string token);
}