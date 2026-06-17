using System.Text.Json;

namespace UserReportService.Api.Middlewares;

/// <summary>
/// Middleware chặn các lỗi phân quyền (401, 403) từ hệ thống JWT Authentication
/// và định dạng lại thành chuỗi JSON chuẩn thống nhất với toàn bộ API.
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;

    public AuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Kiểm tra nếu request đã chạy xong và bị đánh dấu là lỗi xác thực
        if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
        {
            // Tránh lỗi "Response has already started"
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này (Token không hợp lệ hoặc đã bị thiếu)", statusCode = 401 };
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
            }
        }
        else if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var response = new { success = false, message = "Tài khoản của bạn không có đủ quyền (Role) để truy cập tài nguyên này", statusCode = 403 };
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
            }
        }
    }
}
