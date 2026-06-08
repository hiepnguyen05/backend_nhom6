using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserReportService.Application.Interfaces;
using UserReportService.Application.Features.Auth.Commands;
using UserReportService.Domain.Interfaces;
using UserReportService.Infrastructure.Data;
using UserReportService.Infrastructure.Messaging;
using UserReportService.Infrastructure.Repositories;
using UserReportService.Infrastructure.Security;
using UserReportService.Infrastructure.Services;
using UserReportService.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();

            var response = new
            {
                success = false,
                message = "Dữ liệu đầu vào không hợp lệ",
                errors = errors,
                statusCode = 400
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(response);
        };
    });
builder.Services.AddOpenApi();

// DbContext Registration
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("UserReportService.Infrastructure")));

// Repository Registration
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IOrderReceiptRepository, OrderReceiptRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ===================== CQRS: MediatR Registration =====================
// Quét toàn bộ Command/Query Handlers trong assembly Application
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

// ===================== Redis Cache Registration =====================
// Redis chỉ dùng để cache dữ liệu báo cáo (Read Model), KHÔNG cache dữ liệu User/Auth
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    options.InstanceName = "UserReportService:";
});
builder.Services.AddScoped<ICacheService, CacheService>();

// Infrastructure Services
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IEventPublisher, KafkaPublisher>();

// Background Services
builder.Services.AddHostedService<KafkaOrderConsumer>();

// JWT Authentication Configuration
var jwtKey = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is missing");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
        
        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                // Hủy bỏ response mặc định của .NET
                context.HandleResponse();
                
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var response = new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này (Token không hợp lệ hoặc đã bị thiếu)", statusCode = 401 };
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, jsonOptions));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "application/json";
                var response = new { success = false, message = "Tài khoản của bạn không có đủ quyền (Role) để truy cập tài nguyên này", statusCode = 403 };
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };
                await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, jsonOptions));
            }
        };
    });

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Apply migrations and seed data at startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred migrating or seeding the database.");
    }
}

app.Run();
