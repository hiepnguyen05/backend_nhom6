using System.ComponentModel.DataAnnotations;
using UserReportService.Domain.Enums;

namespace UserReportService.Application.DTOs;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string FullName { get; set; }
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public UserStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserRequest
{
    [Required(ErrorMessage = "Email không được để trống")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không hợp lệ")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Mật khẩu không được để trống")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8 ký tự trở lên")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Họ tên không được để trống")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên phải có ít nhất 2 ký tự")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
    public required string FullName { get; set; }

    [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
}

public class UpdateUserRequest
{
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Họ tên phải có ít nhất 2 ký tự")]
    [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên chỉ được chứa chữ cái và khoảng trắng")]
    public string? FullName { get; set; }

    [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }
    public UserStatus? Status { get; set; }
}

public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
    public required string CurrentPassword { get; set; }

    [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu mới phải từ 8 ký tự trở lên")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$", ErrorMessage = "Mật khẩu mới phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public required string NewPassword { get; set; }
}
