using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class IotControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public IotControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task<Guid> CreateTestDeviceAsync(bool isActive = true)
        {
            using IServiceScope scope = _factory.Services.CreateScope();
            ApplicationDbContext? dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Guid deviceId = Guid.NewGuid();
            Device? device = new Device
            {
                Id = deviceId,
                Name = $"Test IoT Device {deviceId.ToString().Substring(0, 5)}",
                IsActive = isActive
            };

            dbContext.Devices.Add(device);
            await dbContext.SaveChangesAsync();

            return deviceId;
        }

        [Fact]
        public async Task RecordData_WithValidHeaderAndData_ReturnsOk()
        {
            Guid deviceId = await CreateTestDeviceAsync(isActive: true);
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                Temperature = 24.5m,
                Humidity = 55.0m
            };

            HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Post, "/api/Iot/data");
            request.Headers.Add("X-Device-Id", deviceId.ToString());
            request.Content = JsonContent.Create(command);

            HttpResponseMessage? response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<long>? result = await response.Content.ReadFromJsonAsync<Response<long>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal("Dữ liệu đã được ghi nhận.", result.Message);
            Assert.True(result.Data > 0);
        }

        [Fact]
        public async Task RecordData_MissingDeviceIdHeader_ReturnsBadRequest()
        {
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                Temperature = 25.0m,
                Humidity = 60.0m
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Iot/data", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains("Thiếu hoặc sai định dạng X-Device-Id", result.Message);
        }

        [Fact]
        public async Task RecordData_InvalidDeviceIdHeaderFormat_ReturnsBadRequest()
        {
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                Temperature = 25.0m,
                Humidity = 60.0m
            };

            HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Post, "/api/Iot/data");
            request.Headers.Add("X-Device-Id", "not-a-valid-guid");
            request.Content = JsonContent.Create(command);

            HttpResponseMessage? response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains("Thiếu hoặc sai định dạng X-Device-Id", result.Message);
        }

        [Fact]
        public async Task RecordData_InvalidData_ReturnsBadRequest_FromValidationBehavior()
        {
            Guid deviceId = await CreateTestDeviceAsync(isActive: true);
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                Temperature = 150.0m,
                Humidity = -10.0m
            };

            HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Post, "/api/Iot/data");
            request.Headers.Add("X-Device-Id", deviceId.ToString());
            request.Content = JsonContent.Create(command);

            HttpResponseMessage? response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string? responseString = await response.Content.ReadAsStringAsync();
            Response<string>? result = JsonSerializer.Deserialize<Response<string>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu đầu vào không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Contains("Nhiệt độ phải nằm trong khoảng từ -50 đến 100 độ C."));
            Assert.Contains(result.Errors, e => e.Contains("Độ ẩm phải nằm trong khoảng từ 0% đến 100%."));
        }

        [Fact]
        public async Task RecordData_InactiveDevice_ReturnsBadRequest_FromHandler()
        {
            Guid deviceId = await CreateTestDeviceAsync(isActive: false);
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                Temperature = 25.0m,
                Humidity = 60.0m
            };

            HttpRequestMessage? request = new HttpRequestMessage(HttpMethod.Post, "/api/Iot/data");
            request.Headers.Add("X-Device-Id", deviceId.ToString());
            request.Content = JsonContent.Create(command);

            HttpResponseMessage? response = await _client.SendAsync(request);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Thiết bị đang bị vô hiệu hóa. Không thể nhận dữ liệu.", result.Message);
        }
    }
}