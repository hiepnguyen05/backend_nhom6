using UserReportService.Application.DTOs;

namespace UserReportService.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(RefreshRequest request);
    Task<UserProfileDto> RegisterAsync(RegisterRequest request);
}
