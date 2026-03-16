using ServerMonitorApp.Application.Features.AccessControl.Commands.AssignRoomAccess;
using ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom;
using ServerMonitorApp.Application.Features.Users.Commands.CreateUser;
using ServerMonitorApp.Application.Wrappers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class UserRoomAccessControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UserRoomAccessControllerTests(CustomWebApplicationFactory factory)
        {
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
            Response<AuthResponseDto>? result = JsonSerializer.Deserialize<Response<AuthResponseDto>>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Data?.AccessToken);
        }

        private async Task<(Guid userId, Guid roomId)> CreateTestUserAndRoomAsync()
        {
            string suffix = Guid.NewGuid().ToString().Substring(0, 8);

            CreateUserCommand? userCmd = new CreateUserCommand 
            { 
                Username = $"accessuser_{suffix}", 
                Email = $"access_{suffix}@test.com", 
                Password = "Password123!", 
                Role = "USER" 
            };
            HttpResponseMessage? userRes = await _client.PostAsJsonAsync("/api/Users", userCmd);
            Guid userId = (await userRes.Content.ReadFromJsonAsync<Response<Guid>>())!.Data;

            CreateRoomCommand? roomCmd = new CreateRoomCommand 
            { 
                Name = $"Access Room {suffix}", 
                Location = "Test Location" 
            };
            HttpResponseMessage? roomRes = await _client.PostAsJsonAsync("/api/Rooms", roomCmd);
            Guid roomId = (await roomRes.Content.ReadFromJsonAsync<Response<Guid>>())!.Data;

            return (userId, roomId);
        }

        [Fact]
        public async Task AssignAccess_WithValidData_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();
            (Guid userId, Guid roomId) = await CreateTestUserAndRoomAsync();

            AssignRoomAccessCommand? command = new AssignRoomAccessCommand
            {
                UserId = userId,
                RoomId = roomId,
                ReceiveAlerts = true
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/access", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();
            Assert.True(result!.Succeeded);
            Assert.Equal("Thành công", result.Data);
        }

        [Fact]
        public async Task GetUsersByRoom_ReturnsAssignedUsers()
        {
            await AuthenticateAsAdminAsync();
            (Guid userId, Guid roomId) = await CreateTestUserAndRoomAsync();

            await _client.PostAsJsonAsync("/api/access", new AssignRoomAccessCommand { UserId = userId, RoomId = roomId, ReceiveAlerts = true });

            HttpResponseMessage? response = await _client.GetAsync($"/api/access/rooms/{roomId}/users");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<IEnumerable<UserRoomAccessDto>>? result = await response.Content.ReadFromJsonAsync<Response<IEnumerable<UserRoomAccessDto>>>();

            Assert.True(result!.Succeeded);
            Assert.Contains(result.Data!, u => u.UserId == userId && u.RoomId == roomId);
        }

        [Fact]
        public async Task UpdateAccess_WithValidData_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();
            (Guid userId, Guid roomId) = await CreateTestUserAndRoomAsync();
            await _client.PostAsJsonAsync("/api/access", new AssignRoomAccessCommand { UserId = userId, RoomId = roomId, ReceiveAlerts = true });

            UpdateRoomAccessCommand? updateCmd = new UpdateRoomAccessCommand
            {
                UserId = userId,
                RoomId = roomId,
                ReceiveAlerts = false
            };

            HttpResponseMessage? response = await _client.PutAsJsonAsync("/api/access", updateCmd);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RevokeAccess_WithValidData_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();
            var (userId, roomId) = await CreateTestUserAndRoomAsync();
            await _client.PostAsJsonAsync("/api/access", new AssignRoomAccessCommand { UserId = userId, RoomId = roomId, ReceiveAlerts = true });

            HttpResponseMessage? response = await _client.DeleteAsync($"/api/access/users/{userId}/rooms/{roomId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            HttpResponseMessage? checkResponse = await _client.GetAsync($"/api/access/rooms/{roomId}/users");
            Response<IEnumerable<UserRoomAccessDto>>? result = await checkResponse.Content.ReadFromJsonAsync<Response<IEnumerable<UserRoomAccessDto>>>();
            Assert.Empty(result!.Data!);
        }
    }
}