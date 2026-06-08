using MediatR;
using UserReportService.Application.DTOs;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Interfaces;

namespace UserReportService.Application.Features.Users.Queries;

// ======================== GET USER BY ID ========================
public record GetUserByIdQuery(Guid Id) : IRequest<UserProfileDto>;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        return MapToProfileDto(user);
    }

    private static UserProfileDto MapToProfileDto(User user) => new()
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

// ======================== GET ALL USERS ========================
public record GetAllUsersQuery() : IRequest<IEnumerable<UserProfileDto>>;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<UserProfileDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(1, 100, null);
        return users.Select(user => new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            Phone = user.Phone,
            Role = user.Role,
            Status = user.Status,
            CreatedAt = user.CreatedAt
        });
    }
}
