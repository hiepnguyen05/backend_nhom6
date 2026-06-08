using MediatR;
using UserReportService.Application.DTOs;
using UserReportService.Application.Interfaces;
using UserReportService.Domain.Entities;
using UserReportService.Domain.Enums;
using UserReportService.Domain.Interfaces;

namespace UserReportService.Application.Features.Users.Commands;

// ======================== CREATE USER ========================
public record CreateUserCommand(string Email, string Password, string FullName, string? Phone, UserRole Role) : IRequest<UserProfileDto>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserProfileDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
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
            Status = UserStatus.Active
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event
        var userCreatedEvent = new UserCreatedEvent(
            user.Id, user.Email, user.FullName, user.Phone, user.Role, user.Status, user.CreatedAt);
        await _eventPublisher.PublishAsync("user.created", userCreatedEvent, cancellationToken);

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

// ======================== UPDATE USER ========================
public record UpdateUserCommand(Guid Id, string? FullName, string? Phone, UserStatus? Status) : IRequest<UserProfileDto>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserProfileDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<UserProfileDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Phone != null) user.Phone = request.Phone;
        if (request.Status.HasValue) user.Status = request.Status.Value;

        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event
        var userUpdatedEvent = new UserUpdatedEvent(
            user.Id, user.FullName, user.Phone, user.Status, user.UpdatedAt ?? DateTime.UtcNow);
        await _eventPublisher.PublishAsync("user.updated", userUpdatedEvent, cancellationToken);

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

// ======================== DELETE USER ========================
public record DeleteUserCommand(Guid Id) : IRequest<Unit>;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork, IEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        await _unitOfWork.Users.DeleteAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish event
        await _eventPublisher.PublishAsync("user.deleted", new UserDeletedEvent(user.Id), cancellationToken);

        return Unit.Value;
    }
}

// ======================== CHANGE PASSWORD ========================
public record ChangePasswordCommand(Guid Id, string CurrentPassword, string NewPassword) : IRequest<Unit>;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
        if (user == null) throw new ArgumentException("Không tìm thấy người dùng");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new ArgumentException("Mật khẩu hiện tại không chính xác");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Users.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
