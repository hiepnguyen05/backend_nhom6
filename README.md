# UserReportService (Backend Nhóm 6)

[![.NET](https://img.shields.io/badge/.NET-8.0%2B-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Supported-2496ED?logo=docker)](https://www.docker.com/)
[![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?logo=redis)](https://redis.io/)
[![SQL Server](https://img.shields.io/badge/SQL_Server-EF_Core-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/en-us/sql-server)

**UserReportService** là một vi dịch vụ (microservice) backend thuộc dự án Quản lý Bán hàng của Nhóm 6. Dịch vụ này chịu trách nhiệm quản lý thông tin người dùng, xử lý xác thực/phân quyền (Authentication & Authorization), tiếp nhận các sự kiện nghiệp vụ (Events) và tổng hợp, xuất báo cáo (Reports) thống kê doanh thu, hóa đơn.

## 🏛️ Kiến trúc & Công nghệ

Dự án được xây dựng dựa trên nguyên lý **Clean Architecture** (Kiến trúc Sạch) kết hợp với mẫu thiết kế **CQRS** (Command Query Responsibility Segregation), giúp mã nguồn dễ dàng mở rộng, bảo trì và kiểm thử.

### Công nghệ sử dụng:
- **Framework:** .NET 8 / ASP.NET Core Web API
- **Kiến trúc:** Clean Architecture, CQRS (với `MediatR`)
- **Cơ sở dữ liệu:** Microsoft SQL Server (via Entity Framework Core)
- **Caching:** Redis Cache (Tối ưu hóa tốc độ đọc dữ liệu báo cáo)
- **Bảo mật:** JSON Web Token (JWT) cho Authentication
- **Tài liệu API:** OpenAPI / Swagger
- **Triển khai:** Docker & Docker Compose

## 📂 Cấu trúc thư mục

- `Api/`: Lớp Presentation (REST API, Controllers, Middlewares, Dependency Injection).
- `Application/`: Lớp Business Logic chứa các Use Cases, triển khai CQRS (Commands, Queries) và DTOs.
- `Domain/`: Lớp Core chứa các Entities, Enums và Interfaces cốt lõi.
- `Infrastructure/`: Lớp hạ tầng chứa cấu hình Database (EF Core), Caching, Security và Data Repositories.
- `Tests/`: Chứa các Unit Test và Integration Test.

## 🚀 Hướng dẫn cài đặt và chạy dự án

### Yêu cầu hệ thống (Prerequisites)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker & Docker Compose](https://www.docker.com/) (Khuyến nghị)
- SQL Server (Hoặc chạy qua Docker)
- Redis Server (Hoặc chạy qua Docker)

### Cách 1: Chạy bằng Docker (Nhanh nhất)
Dự án đã được cấu hình sẵn `docker-compose.yml` bao gồm ứng dụng web, cơ sở dữ liệu SQL Server và Redis.
```bash
# Clone dự án
git clone https://github.com/hiepnguyen05/backend_nhom6.git
cd backend_nhom6

# Build và khởi chạy các container
docker-compose up -d --build
```
*API sẽ lắng nghe ở cổng `80` hoặc `5000` (theo cấu hình của Docker).*

### Cách 2: Chạy Local (Môi trường phát triển)
1. Cập nhật chuỗi kết nối Database và cấu hình Redis trong file `Api/appsettings.json` hoặc `Api/appsettings.Development.json`.
2. Mở terminal tại thư mục gốc và chạy lệnh cập nhật database (Migrations):
   ```bash
   dotnet ef database update --project Infrastructure --startup-project Api
   ```
3. Khởi chạy dự án:
   ```bash
   cd Api
   dotnet run
   ```

## 📖 Tài liệu API (Swagger)

Khi dự án đang chạy ở môi trường Development, bạn có thể truy cập giao diện Swagger UI để xem và thử nghiệm trực tiếp các API:
- **URL mặc định:** `http://localhost:<port>/swagger` (hoặc `/openapi`)

## 👥 Nhóm phát triển
- **Nhóm 6** - Dự án FullStack BTL.
