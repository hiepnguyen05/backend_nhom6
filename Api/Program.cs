using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserReportService.Application.Interfaces;
using UserReportService.Application.Features.Auth.Commands;
using UserReportService.Domain.Interfaces;
using UserReportService.Infrastructure.Data;
using UserReportService.Infrastructure.Repositories;
using UserReportService.Infrastructure.Security;
using UserReportService.Infrastructure.Services;
using UserReportService.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Force Kestrel to listen on both 80 and 5000, overriding docker-compose env vars
builder.Configuration["ASPNETCORE_HTTP_PORTS"] = "80;5000";

// Ensure it listens on both 80 (for external map) and 5000 (for internal gateway)
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(80);
    options.ListenAnyIP(5000);
});

// Add CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://127.0.0.1:3000", "http://127.0.0.1:5173", "https://braylen-noisiest-biennially.ngrok-free.dev")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add services to the container.
builder.Services.AddRouting(options => options.LowercaseUrls = true); // Ép toàn bộ URL thành chữ thường (lowercase)
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
    });

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

// Sử dụng Middleware tự viết để xử lý lỗi 401/403 thay vì nhét chung vào khai báo JWT
app.UseMiddleware<AuthMiddleware>();

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
