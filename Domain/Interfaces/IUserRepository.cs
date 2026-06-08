using UserReportService.Domain.Entities;
using UserReportService.Domain.Enums;

namespace UserReportService.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync(int page, int pageSize, UserRole? role);
    Task<int> GetCountAsync(UserRole? role);
    Task AddAsync(User user);
    Task<bool> IsEmailUniqueAsync(string email);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}
