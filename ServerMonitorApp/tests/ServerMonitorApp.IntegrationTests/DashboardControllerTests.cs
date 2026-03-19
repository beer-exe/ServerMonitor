using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class DashboardControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public DashboardControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsAdminAsync()
        {
            LoginCommand loginCommand = new LoginCommand
            {
                UsernameOrEmail = "integrationuser",
                Password = "Password123!"
            };

            HttpResponseMessage response = await _client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            Response<AuthResponseDto>? result = JsonSerializer.Deserialize<Response<AuthResponseDto>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Data?.AccessToken);
        }

        private async Task<(Guid roomId, Guid deviceId)> SeedDashboardDataAsync()
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Guid roomId = Guid.NewGuid();
            Guid deviceId = Guid.NewGuid();

            dbContext.Rooms.Add(new Room
            {
                Id = roomId,
                Name = $"Dash Room {roomId.ToString().Substring(0, 5)}"
            });

            dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                RoomId = roomId,
                Name = "Dash Sensor",
                IsActive = true,
                LastSeen = DateTime.UtcNow
            });

            DateTime baseTime = DateTime.UtcNow;
            dbContext.SensorDatas.AddRange(new List<SensorData>
            {
                new SensorData { DeviceId = deviceId, Temperature = 22.0m, Humidity = 50.0m, Timestamp = baseTime.AddHours(-3) },
                new SensorData { DeviceId = deviceId, Temperature = 23.5m, Humidity = 55.0m, Timestamp = baseTime.AddHours(-2) },
                new SensorData { DeviceId = deviceId, Temperature = 24.0m, Humidity = 56.0m, Timestamp = baseTime.AddHours(-1) }
            });

            await dbContext.SaveChangesAsync();

            return (roomId, deviceId);
        }

        [Fact]
        public async Task Endpoints_WhenNotAuthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            HttpResponseMessage dashboardResponse = await _client.GetAsync("/api/Dashboard");
            HttpResponseMessage historyResponse = await _client.GetAsync($"/api/Dashboard/history/devices/{Guid.NewGuid()}");

            Assert.Equal(HttpStatusCode.Unauthorized, dashboardResponse.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, historyResponse.StatusCode);
        }

        [Fact]
        public async Task GetDashboard_WhenAuthenticated_ReturnsOkAndData()
        {
            await AuthenticateAsAdminAsync();
            var (roomId, deviceId) = await SeedDashboardDataAsync();

            HttpResponseMessage response = await _client.GetAsync("/api/Dashboard");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Response<IEnumerable<DashboardRoomDto>>? result = await response.Content.ReadFromJsonAsync<Response<IEnumerable<DashboardRoomDto>>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);

            DashboardRoomDto? seededRoom = result.Data.FirstOrDefault(r => r.Id == roomId);
            Assert.NotNull(seededRoom);

            DashboardDeviceDto? seededDevice = seededRoom.Devices.FirstOrDefault(d => d.Id == deviceId);
            Assert.NotNull(seededDevice);
            Assert.Equal("Dash Sensor", seededDevice.Name);
            Assert.False(seededDevice.IsOffline);
            Assert.Equal(24.0m, seededDevice.CurrentTemperature);
            Assert.Equal(56.0m, seededDevice.CurrentHumidity);
        }

        [Fact]
        public async Task GetHistoricalData_WithValidDeviceAndDateRange_ReturnsOkAndPagedData()
        {
            await AuthenticateAsAdminAsync();
            var (_, deviceId) = await SeedDashboardDataAsync();

            string startTime = DateTime.UtcNow.AddDays(-1).ToString("O");
            string endTime = DateTime.UtcNow.AddDays(1).ToString("O");
            int pageNumber = 1;
            int pageSize = 2;

            string url = $"/api/Dashboard/history/devices/{deviceId}?startTime={startTime}&endTime={endTime}&pageNumber={pageNumber}&pageSize={pageSize}";

            HttpResponseMessage response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            PagedResponse<IEnumerable<ChartDataPointDto>>? result = await response.Content.ReadFromJsonAsync<PagedResponse<IEnumerable<ChartDataPointDto>>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(3, result.TotalRecords);
            Assert.Equal(2, result.TotalPages);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetHistoricalData_WithInvalidDateRange_ReturnsBadRequest()
        {
            await AuthenticateAsAdminAsync();
            Guid deviceId = Guid.NewGuid();

            string startTime = DateTime.UtcNow.ToString("O");
            string endTime = DateTime.UtcNow.AddDays(-1).ToString("O");

            string url = $"/api/Dashboard/history/devices/{deviceId}?startTime={startTime}&endTime={endTime}&pageNumber=1&pageSize=50";

            HttpResponseMessage response = await _client.GetAsync(url);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string responseString = await response.Content.ReadAsStringAsync();
            Response<string>? result = JsonSerializer.Deserialize<Response<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu đầu vào không hợp lệ.", result.Message);
            Assert.Contains(result.Errors!, e => e.Contains("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc."));
        }
    }
}