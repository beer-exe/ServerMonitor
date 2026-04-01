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
* **Giao diện Web tương tác (Client SPA):**
    * Quản lý danh mục Phòng máy (Rooms), Thiết bị (Devices) và Người dùng (Users).
    * Cung cấp biểu đồ trực quan (Recharts) để xem lại dữ liệu lịch sử của thiết bị.    

---

## Công nghệ & Kiến trúc

Dự án được cấu trúc theo **Clean Architecture** và **CQRS Pattern** cho phần Backend để đảm bảo tính độc lập, dễ bảo trì và mở rộng, kết hợp với Single Page Application (SPA) cho phần Frontend.

### 1. Backend (API)
* **Framework:** .NET 8 (ASP.NET Core Web API)
* **Cơ sở dữ liệu:** PostgreSQL (qua Entity Framework Core)
* **Thư viện/Công cụ được sử dụng::**
    * **Entity Framework Core (PostgreSQL):** ORM thao tác với cơ sở dữ liệu.
    * **MediatR:** Triển khai CQRS (Command/Query Responsibility Segregation).
    * **FluentValidation:** Validate dữ liệu đầu vào tự động thông qua Pipeline Behavior.
    * **SignalR:** WebSockets hỗ trợ real-time cho Dashboard và Cảnh báo.
    * **MailKit & MimeKit:** Xử lý tác vụ gửi email thông báo.
    * **BCrypt:** Mã hóa mật khẩu.
* **Testing:** xUnit & Moq (Framework phục vụ Unit Test và Integration Test.).

### 2. Frontend (Client)
* **Framework:** ReactJS 19 (với Vite)
* **Ngôn ngữ:** TypeScript
* **UI/UX:** Tailwind CSS, Ant Design (antd)
* **Thư viện/Công cụ được sử dụng::**
    * **Recharts:** Vẽ biểu đồ trực quan cho dữ liệu lịch sử giám sát (nhiệt độ, độ ẩm).
    * **Axios:** Gọi API Backend (xử lý HTTP requests, tự động đính kèm và làm mới JWT Token).
    * **@microsoft/signalr:** WebSockets hỗ trợ real-time lắng nghe luồng dữ liệu cảm biến và nhận cảnh báo tức thời từ server.

---

## Cấu trúc dự án

```text
ServerMonitorApp.sln
├── src/
│   ├── ServerMonitorApp.API/            # Controllers, SignalR Hubs, Background Services
│   ├── ServerMonitorApp.Application/    # CQRS (Commands/Queries), DTOs, Validators, Interfaces
│   ├── ServerMonitorApp.Domain/         # Core Models (User, Room, Device, SensorData,...), Exceptions
│   ├── ServerMonitorApp.Infrastructure/ # DbContext (PostgreSQL), Services (Email, JWT)
│   └── ServerMonitorApp.Client/         # Web Client Frontend (ReactJS + Vite)
└── tests/
    ├── ServerMonitorApp.IntegrationTests/ # Test tích hợp API (sử dụng InMemory Db)
    └── ServerMonitorApp.UnitTests/        # Test chức năng từng thành phần (Handlers, Validators)
```

## Hướng dẫn cài đặt và chạy ứng dụng

### 1. Yêu cầu hệ thống
* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [PostgreSQL](https://www.postgresql.org/download/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/downloads/).
* [Visual Studio Code](https://code.visualstudio.com/).

### 2. Cài đặt Backend (API)
2.1 **Cấu hình môi trường**
* Đổi tên file `appsettings.Example.json` trong project `ServerMonitorApp.API` thành `appsettings.json` và cập nhật các thông số cho phù hợp với môi trường của bạn:

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

2.2 **Cập nhật Database (Migration)**
* Mở terminal tại thư mục root của solution và chạy lệnh sau để khởi tạo cơ sở dữ liệu:

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

2.3 **Khởi chạy ứng dụng**
* Chạy lệnh sau để khởi động API:

```bash
dotnet run --project src/ServerMonitorApp.API
```

* **API sẽ khởi chạy với Swagger UI: http://localhost:5269/swagger**

### 3. Cài đặt Frontend (Client)
3.1 **Cấu hình biến môi trường**
* Kiểm tra file .env trong thư mục src/ServerMonitorApp.Client và đảm bảo biến môi trường đang trỏ tới đúng cổng của API:
```bash
VITE_API_URL=http://localhost:5269
```

3.2 **Cài đặt dependencies và chạy Web UI**
* Mở một cửa sổ terminal tại đây và chạy lần lượt các lệnh sau:
```bash
# 1. Di chuyển vào thư mục chứa mã nguồn
cd src/ServerMonitorApp.Client

# 2. Tải và cài đặt tất cả các thư viện, (dependencies) cần thiết
npm install

# 3. Khởi chạy giao diện
npm run dev
```
* **Giao diện sẽ khởi chạy tại: http://localhost:3000**
