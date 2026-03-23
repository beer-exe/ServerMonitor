using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Infrastructure.Persistence
{
    public static class ApplicationDbContextSeed
    {
        public static async Task SeedSampleDataAsync(ApplicationDbContext context, IPasswordHasher passwordHasher, ILogger logger)
        {
            logger.LogInformation("Đang tiến hành Seed Data cho toàn bộ hệ thống...");

            Guid adminId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            Guid roomAId = Guid.NewGuid();
            Guid roomBId = Guid.NewGuid();

            Guid device1Id = Guid.NewGuid();
            Guid device2Id = Guid.NewGuid();
            Guid device3Id = Guid.NewGuid();

            DateTime now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (!await context.Users.AnyAsync())
            {
                List<User>? users = new List<User>
                {
                    new User
                    {
                        Id = adminId,
                        Username = "admin",
                        Email = "admin@servermonitor.com",
                        PasswordHash = passwordHasher.HashPassword("Admin@123!"),
                        Role = "ADMIN",
                        CreatedAt = now,
                        UpdatedAt = now
                    },
                    new User
                    {
                        Id = userId,
                        Username = "user1",
                        Email = "user1@servermonitor.com",
                        PasswordHash = passwordHasher.HashPassword("User@123!"),
                        Role = "USER",
                        CreatedAt = now,
                        UpdatedAt = now
                    }
                };
                context.Users.AddRange(users);
                logger.LogInformation("Đã tạo tài khoản Admin và User mẫu.");
            }
            else
            {
                adminId = await context.Users.Where(u => u.Role == "ADMIN").Select(u => u.Id).FirstOrDefaultAsync();
                userId = await context.Users.Where(u => u.Role == "USER").Select(u => u.Id).FirstOrDefaultAsync();
            }

            if (!await context.Rooms.AnyAsync())
            {
                List<Room>? rooms = new List<Room>
                {
                    new Room { Id = roomAId, Name = "Phòng Máy Chủ A", Location = "Tầng 1 - Tòa nhà Chính" },
                    new Room { Id = roomBId, Name = "Phòng Mạng B", Location = "Tầng 2 - Tòa nhà Phụ" }
                };
                context.Rooms.AddRange(rooms);
                logger.LogInformation("Đã tạo các phòng máy mẫu.");
            }
            else
            {
                List<Room>? existingRooms = await context.Rooms.Take(2).ToListAsync();
                if (existingRooms.Count > 0) roomAId = existingRooms[0].Id;
                if (existingRooms.Count > 1) roomBId = existingRooms[1].Id;
            }

            await context.SaveChangesAsync();

            if (!await context.Devices.AnyAsync())
            {
                List<Device>? devices = new List<Device>
                {
                    new Device
                    {
                        Id = device1Id,
                        RoomId = roomAId,
                        Name = "Cảm biến Nhiệt/Ẩm Tủ Rack 1",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        WarningTemp = 25.0m,
                        CriticalTemp = 30.0m,
                        WarningHumidity = 60.0m,
                        CriticalHumidity = 80.0m,
                        LastSeen = now
                    },
                    new Device
                    {
                        Id = device2Id,
                        RoomId = roomAId,
                        Name = "Cảm biến Nhiệt/Ẩm Tủ Rack 2",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        WarningTemp = 25.0m,
                        CriticalTemp = 30.0m,
                        WarningHumidity = 60.0m,
                        CriticalHumidity = 80.0m,
                        LastSeen = now.AddMinutes(-10)
                    },
                    new Device
                    {
                        Id = device3Id,
                        RoomId = roomBId,
                        Name = "Cảm biến Tổng Phòng B",
                        IsActive = true,
                        CreatedAt = now,
                        UpdatedAt = now,
                        WarningTemp = 28.0m,
                        CriticalTemp = 35.0m,
                        WarningHumidity = 65.0m,
                        CriticalHumidity = 85.0m,
                        LastSeen = now
                    }
                };
                context.Devices.AddRange(devices);
                logger.LogInformation("Đã tạo các thiết bị IoT mẫu cùng các ngưỡng cảnh báo.");
            }
            else
            {
                device1Id = await context.Devices.Select(d => d.Id).FirstOrDefaultAsync();
            }

            if (!await context.UserRoomAccesses.AnyAsync())
            {
                List<UserRoomAccess>? accesses = new List<UserRoomAccess>
                {
                    new UserRoomAccess { UserId = adminId, RoomId = roomAId, ReceiveAlerts = true },
                    new UserRoomAccess { UserId = adminId, RoomId = roomBId, ReceiveAlerts = true },
                    new UserRoomAccess { UserId = userId, RoomId = roomAId, ReceiveAlerts = true }
                };
                context.UserRoomAccesses.AddRange(accesses);
                logger.LogInformation("Đã thiết lập phân quyền quản lý phòng máy.");
            }

            if (!await context.SensorDatas.AnyAsync())
            {
                List<SensorData>? sensorDatas = new List<SensorData>();
                long sensorDataId = 1;

                for (int i = 5; i >= 0; i--)
                {
                    sensorDatas.Add(new SensorData
                    {
                        Id = sensorDataId++,
                        DeviceId = device1Id,
                        Temperature = 26.5m + (decimal)(new Random().NextDouble() * 2),
                        Humidity = 45.0m + (decimal)(new Random().NextDouble() * 5),
                        Timestamp = now.AddMinutes(-i * 5)
                    });
                }
                context.SensorDatas.AddRange(sensorDatas);
                logger.LogInformation("Đã tạo dữ liệu cảm biến (Sensor Data) lịch sử mẫu.");
            }

            if (!await context.Alerts.AnyAsync())
            {
                List<Alert>? alerts = new List<Alert>
                {
                    new Alert
                    {
                        DeviceId = device1Id,
                        RoomId = roomAId,
                        Message = "Nhiệt độ tủ Rack 1 vượt ngưỡng cảnh báo (Lên tới 28.1 độ C).",
                        Severity = "WARNING",
                        IsResolved = false,
                        CreatedAt = now
                    }
                };
                context.Alerts.AddRange(alerts);
                logger.LogInformation("Đã tạo cảnh báo sự cố (Alert) mẫu.");
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Hoàn tất lưu toàn bộ dữ liệu Seed vào Database.");
        }
    }
}