using System;
using UserReportService.Domain.Enums;

namespace UserReportService.Application.DTOs;

public record UserCreatedEvent(
    Guid Id,
    string Email,
    string FullName,
    string? Phone,
    UserRole Role,
    UserStatus Status,
    DateTime CreatedAt
);

public record UserUpdatedEvent(
    Guid Id,
    string FullName,
    string? Phone,
    UserStatus Status,
    DateTime UpdatedAt
);

public record UserDeletedEvent(
    Guid Id
);
