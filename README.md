# Server Monitor App (Hệ thống Giám sát Phòng Máy Chủ)

Server Monitor App là một hệ thống backend API được xây dựng trên nền tảng **.NET 8** ứng dụng mô hình **Clean Architecture** và mẫu thiết kế **CQRS**. Ứng dụng cung cấp giải pháp toàn diện để quản lý, giám sát các thiết bị IoT trong phòng máy chủ, nhận dữ liệu cảm biến (nhiệt độ, độ ẩm) theo thời gian thực, và tự động cảnh báo khi có sự cố.

---

## Các tính năng chính

* **Quản lý dữ liệu IoT theo thời gian thực:**
    * Tiếp nhận dữ liệu nhiệt độ, độ ẩm từ các thiết bị IoT.
    * Cập nhật dữ liệu tức thời lên Dashboard thông qua **SignalR**.
* **Hệ thống cảnh báo thông minh (Alert System):**
    * Tự động phát hiện và sinh cảnh báo khi nhiệt độ/độ ẩm vượt ngưỡng (Warning/Critical).
    * Background Worker (`DeviceStatusMonitorWorker`) chạy ngầm định kỳ kiểm tra và phát hiện các thiết bị mất kết nối (Offline).
    * Gửi thông báo cảnh báo tức thời qua **SignalR** và **Email** (sử dụng MailKit).
    * Tiếp nhận và xử lý sự cố (Resolve Alert).
* **Xác thực và Phân quyền (Auth & Access Control):**
    * Xác thực người dùng bằng **JWT Token** (hỗ trợ Access Token và Refresh Token).
    * Mã hóa mật khẩu an toàn với **BCrypt**.
    * Phân quyền theo Role (ADMIN/USER). ADMIN có toàn quyền, USER chỉ được giám sát các phòng được chỉ định (UserRoomAccess).
* **Quản lý danh mục:**
    * Quản lý thông tin Phòng máy (Rooms) và Thiết bị (Devices).
    * Quản lý người dùng (Users) và phân quyền quản lý phòng máy.
* **Báo cáo & Lịch sử:**
    * Cung cấp dữ liệu lịch sử để vẽ biểu đồ theo dõi nhiệt độ, độ ẩm theo thời gian.

---

## Công nghệ & Kiến trúc

Dự án được cấu trúc theo **Clean Architecture** để đảm bảo tính độc lập, dễ bảo trì và mở rộng.

* **Framework:** .NET 8 (ASP.NET Core Web API)
* **Kiến trúc:** Clean Architecture, CQRS Pattern
* **Thư viện/Công cụ được sử dụng:**
    * **MediatR:** Triển khai CQRS (Command/Query Responsibility Segregation).
    * **Entity Framework Core (PostgreSQL):** ORM thao tác với cơ sở dữ liệu.
    * **FluentValidation:** Validate dữ liệu đầu vào tự động thông qua Pipeline Behavior.
    * **SignalR:** WebSockets hỗ trợ real-time cho Dashboard và Cảnh báo.
    * **MailKit & MimeKit:** Xử lý tác vụ gửi email thông báo.
    * **xUnit & Moq:** Framework phục vụ Unit Test và Integration Test.

---

## Cấu trúc dự án

```text
ServerMonitorApp.sln
├── src/
│   ├── ServerMonitorApp.API/            #Controllers, SignalR Hubs, Middlewares, Background Services
│   ├── ServerMonitorApp.Application/    #CQRS (Commands, Queries, Event Handlers), DTOs, Interfaces, Validators
│   ├── ServerMonitorApp.Domain/         #Models (User, Room, Device, Alert, SensorData,...), Exceptions
│   └── ServerMonitorApp.Infrastructure/ #DbContext (EF Core), Services (Email, JWT, PasswordHasher)
└── tests/
    ├── ServerMonitorApp.IntegrationTests/ # Test tích hợp API (sử dụng InMemory Db)
    └── ServerMonitorApp.UnitTests/        # Test chức năng từng thành phần (Handlers, Validators)
```

## Hướng dẫn cài đặt và chạy ứng dụng

### 1. Yêu cầu hệ thống
* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [PostgreSQL](https://www.postgresql.org/download/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/).

### 2. Cấu hình môi trường
Đổi tên file `appsettings.Example.json` trong project `ServerMonitorApp.API` thành `appsettings.json` và cập nhật các thông số cho phù hợp với môi trường của bạn:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "DefaultConnection": "Host=<DB_HOST>:<DB_PORT>;Database=<DB_NAME>;Username=<DB_USER>;Password=<DB_PASSWORD>"
  },

  "JwtSettings": {
    "SecretKey": "<YOUR_SECRET_KEY>",
    "Issuer": "ServerMonitorApp.API",
    "Audience": "ServerMonitorApp.Client",
    "ExpirationMinutes": 10,
    "RefreshTokenExpirationDays": 7
  },

  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": "587",
    "SmtpUser": "<YOUR_EMAIL@gmail.com>",
    "SmtpPass": "<YOUR_APP_PASSWORD>",
    "FromEmail": "no-reply@servermonitor.com",
    "FromName": "Server Monitor System"
  }
}
```

### 3. Cập nhật Database (Migration)
Mở terminal tại thư mục root của solution và chạy lệnh sau để khởi tạo cơ sở dữ liệu:

```bash
# (Tùy chọn) Cài đặt công cụ EF Core nếu máy bạn chưa có
dotnet tool install --global dotnet-ef --version 8.

# Di chuyển vào thư mục project API (nơi chứa cấu hình DbContext)
cd src/ServerMonitorApp.API

# Tạo file migration đầu tiên để khởi tạo cấu trúc các bảng trong Database
dotnet ef migrations add InitialCreate --project ../ServerMonitorApp.Infrastructure --startup-project .

# Cập nhật Database dựa trên Migrations vừa tạo
dotnet ef database update --project ../ServerMonitorApp.Infrastructure --startup-project .

# Nạp dữ liệu mẫu cho database
dotnet run -- /seed
```

### 4. Khởi chạy ứng dụng
Chạy lệnh sau để khởi động API:

```bash
dotnet run --project src/ServerMonitorApp.API
```

Ứng dụng sẽ chạy tại Swagger UI: https://localhost:5269/swagger
