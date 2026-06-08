using UserReportService.Application.DTOs;
using UserReportService.Application.Interfaces;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;
using BCrypt.Net;

namespace UserReportService.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> GetUserByIdAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        return MapToProfileDto(user);
    }

    public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync(1, 100, null);
        return users.Select(MapToProfileDto);
    }

    public async Task<UserProfileDto> CreateUserAsync(CreateUserRequest request)
    {
        if (await _unitOfWork.Users.IsEmailUniqueAsync(request.Email) == false)
            throw new ArgumentException("Email này đã được sử dụng");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = request.FullName,
            Phone = request.Phone,
            Role = request.Role,
            Status = Domain.Enums.UserStatus.Active
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToProfileDto(user);
    }

    public async Task<UserProfileDto> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.Status.HasValue) user.Status = request.Status.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapToProfileDto(user);
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Mật khẩu hiện tại không chính xác");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateUserStatusAsync(Guid id, Domain.Enums.UserStatus status)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        user.Status = status;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private static UserProfileDto MapToProfileDto(User user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        };
    }
}
