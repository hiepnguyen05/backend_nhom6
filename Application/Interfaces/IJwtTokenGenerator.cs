using UserReportService.Domain.Entities;

namespace UserReportService.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
