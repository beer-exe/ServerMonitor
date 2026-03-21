using Microsoft.Extensions.DependencyInjection;
using ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class AlertsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AlertsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task AuthenticateAsAdminAsync()
        {
            LoginCommand? loginCommand = new LoginCommand
            {
                UsernameOrEmail = "integrationuser",
                Password = "Password123!"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            response.EnsureSuccessStatusCode();

            string? responseString = await response.Content.ReadAsStringAsync();
            Response<AuthResponseDto>? result = JsonSerializer.Deserialize<Response<AuthResponseDto>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Data?.AccessToken);
        }

        private async Task<(Guid userId, string token)> CreateAndAuthenticateNormalUserAsync()
        {
            using IServiceScope? scope = _factory.Services.CreateScope();
            ApplicationDbContext? db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Guid userId = Guid.NewGuid();
            string? username = $"user_{userId.ToString()[..8]}";
            string? password = "Password123!";

            db.Users.Add(new User
            {
                Id = userId,
                Username = username,
                Email = $"{username}@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "USER"
            });
            await db.SaveChangesAsync();

            LoginCommand? loginCommand = new LoginCommand { UsernameOrEmail = username, Password = password };
            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Auth/login", loginCommand);
            Response<AuthResponseDto>? result = await response.Content.ReadFromJsonAsync<Response<AuthResponseDto>>();

            return (userId, result!.Data!.AccessToken);
        }

        private async Task<long> SeedAlertAsync()
        {
            using IServiceScope? scope = _factory.Services.CreateScope();
            ApplicationDbContext? db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Guid roomId = Guid.NewGuid();
            db.Rooms.Add(new Room { Id = roomId, Name = "Phòng Test Integration" });

            Alert? alert = new Alert
            {
                RoomId = roomId,
                Message = "Cảnh báo nhiệt độ Integration Test",
                Severity = "CRITICAL",
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };

            db.Alerts.Add(alert);
            await db.SaveChangesAsync();

            return alert.Id;
        }

        [Fact]
        public async Task GetAlerts_WithoutToken_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            HttpResponseMessage? response = await _client.GetAsync("/api/alerts");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAlerts_WithValidToken_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();

            HttpResponseMessage? response = await _client.GetAsync("/api/alerts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PagedResponse<IEnumerable<AlertDto>>? result = await response.Content.ReadFromJsonAsync<PagedResponse<IEnumerable<AlertDto>>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetAlertById_AsAdmin_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();
            long alertId = await SeedAlertAsync();

            HttpResponseMessage? response = await _client.GetAsync($"/api/alerts/{alertId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<AlertDto>? result = await response.Content.ReadFromJsonAsync<Response<AlertDto>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(alertId, result.Data!.Id);
        }

        [Fact]
        public async Task GetAlertById_AsUnassignedUser_ReturnsUnauthorized()
        {
            (Guid userId, string? token) = await CreateAndAuthenticateNormalUserAsync();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            long alertId = await SeedAlertAsync();

            HttpResponseMessage? response = await _client.GetAsync($"/api/alerts/{alertId}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();
            Assert.Contains("Bạn không có quyền", result!.Message);
        }

        [Fact]
        public async Task ResolveAlert_WithValidData_ReturnsOkAndUpdatesDatabase()
        {
            await AuthenticateAsAdminAsync();
            long alertId = await SeedAlertAsync();

            ResolveAlertCommand? command = new ResolveAlertCommand
            {
                ResolutionNote = "Đã sửa xong điều hòa trong test."
            };

            HttpResponseMessage? response = await _client.PutAsJsonAsync($"/api/alerts/{alertId}/resolve", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<long>? result = await response.Content.ReadFromJsonAsync<Response<long>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(alertId, result.Data);

            using IServiceScope? scope = _factory.Services.CreateScope();
            ApplicationDbContext? db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Alert? updatedAlert = await db.Alerts.FindAsync(alertId);

            Assert.NotNull(updatedAlert);
            Assert.True(updatedAlert.IsResolved);
            Assert.Contains("Đã sửa xong điều hòa trong test.", updatedAlert.Message);
        }
    }
}