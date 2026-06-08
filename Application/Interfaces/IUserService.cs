using UserReportService.Application.DTOs;

namespace UserReportService.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetUserByIdAsync(Guid id);
    Task<IEnumerable<UserProfileDto>> GetAllUsersAsync();
    Task<UserProfileDto> CreateUserAsync(CreateUserRequest request);
    Task<UserProfileDto> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task DeleteUserAsync(Guid id);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequest request);
    Task UpdateUserStatusAsync(Guid id, Domain.Enums.UserStatus status);
}
