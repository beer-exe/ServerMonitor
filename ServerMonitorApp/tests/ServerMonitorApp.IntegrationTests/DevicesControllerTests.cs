using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice;
using ServerMonitorApp.Application.Features.Devices.Commands.UpdateDevice;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class DevicesControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public DevicesControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsAdminAsync()
        {
            using (IServiceScope? scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                IPasswordHasher? passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

                if (!context.Users.Any(u => u.Username == "admin_test"))
                {
                    context.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "admin_test",
                        Email = "admin@test.com",
                        PasswordHash = passwordHasher.HashPassword("Admin@123"),
                        Role = "ADMIN"
                    });
                    await context.SaveChangesAsync();
                }
            }

            var loginCommand = new { UsernameOrEmail = "admin_test", Password = "Admin@123" };
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", loginCommand);

            response.EnsureSuccessStatusCode();
            Response<AuthResponseDto>? result = await response.Content.ReadFromJsonAsync<Response<AuthResponseDto>>();

            if (result != null && result.Data != null)
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result.Data.AccessToken);
            }
        }

        private async Task<Guid> SeedRoomAsync()
        {
            using IServiceScope? scope = _factory.Services.CreateScope();
            ApplicationDbContext? context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Room? room = new Room { Id = Guid.NewGuid(), Name = "Phòng Mạng IT" };
            context.Rooms.Add(room);
            await context.SaveChangesAsync();

            return room.Id;
        }

        [Fact]
        public async Task GetDevices_ReturnsOkAndData()
        {
            await AuthenticateAsAdminAsync();

            HttpResponseMessage? response = await _client.GetAsync("/api/devices");

            response.EnsureSuccessStatusCode();
            Response<IEnumerable<DeviceDto>>? result = await response.Content.ReadFromJsonAsync<Response<IEnumerable<DeviceDto>>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task CreateDevice_WithValidData_ReturnsOkAndDeviceId()
        {
            await AuthenticateAsAdminAsync();

            Guid roomId = await SeedRoomAsync();
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Sensor Tầng 1",
                RoomId = roomId,
                IsActive = true,
                TemperatureWarningThreshold = 30,
                TemperatureCriticalThreshold = 40
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/devices", command);

            response.EnsureSuccessStatusCode();
            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.True(Guid.TryParse(result.Data, out _));
        }

        [Fact]
        public async Task UpdateDevice_WithValidData_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();

            Guid roomId = await SeedRoomAsync();
            Guid deviceId = Guid.NewGuid();

            using (IServiceScope? scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Devices.Add(new Device { Id = deviceId, Name = "Sensor Cũ", RoomId = roomId });
                await context.SaveChangesAsync();
            }

            UpdateDeviceCommand? updateCommand = new UpdateDeviceCommand
            {
                Id = deviceId,
                Name = "Sensor Mới",
                RoomId = roomId,
                IsActive = true,
                TemperatureWarningThreshold = 25,
                TemperatureCriticalThreshold = 35
            };

            HttpResponseMessage? response = await _client.PutAsJsonAsync($"/api/devices/{deviceId}", updateCommand);

            response.EnsureSuccessStatusCode();
            Response<bool>? result = await response.Content.ReadFromJsonAsync<Response<bool>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);

            using (IServiceScope? scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Device? deviceInDb = await context.Devices.FindAsync(deviceId);
                Assert.Equal("Sensor Mới", deviceInDb.Name);
            }
        }

        [Fact]
        public async Task DeleteDevice_ExistingDevice_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();

            Guid deviceId = Guid.NewGuid();

            using (IServiceScope? scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Devices.Add(new Device { Id = deviceId, Name = "Cảm biến cần xóa" });
                await context.SaveChangesAsync();
            }

            HttpResponseMessage? response = await _client.DeleteAsync($"/api/devices/{deviceId}");

            response.EnsureSuccessStatusCode();
            Response<bool>? result = await response.Content.ReadFromJsonAsync<Response<bool>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);

            using (IServiceScope? scope = _factory.Services.CreateScope())
            {
                ApplicationDbContext? context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                Device? deviceInDb = await context.Devices.FindAsync(deviceId);
                Assert.Null(deviceInDb);
            }
        }

        [Fact]
        public async Task CreateDevice_WithEmptyName_ReturnsBadRequest()
        {
            await AuthenticateAsAdminAsync();

            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "",
                IsActive = true
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/devices", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Contains("Tên thiết bị là bắt buộc."));
        }
    }
}