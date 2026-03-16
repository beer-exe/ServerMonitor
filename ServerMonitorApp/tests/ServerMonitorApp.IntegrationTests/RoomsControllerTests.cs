using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom;
using ServerMonitorApp.Application.Features.Rooms.Commands.UpdateRoom;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Wrappers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class RoomsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public RoomsControllerTests(CustomWebApplicationFactory factory)
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
            Response<AuthResponseDto>? result = JsonSerializer.Deserialize<Response<AuthResponseDto>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result?.Data?.AccessToken);
        }

        [Fact]
        public async Task Endpoints_WhenNotAuthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            HttpResponseMessage? response = await _client.GetAsync("/api/Rooms");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateRoom_WithValidData_ReturnsOkAndCreatesRoom()
        {
            await AuthenticateAsAdminAsync();
            string? uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = $"Phòng Server {uniqueSuffix}",
                Location = $"Tầng {uniqueSuffix}"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Rooms", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<Guid>? result = await response.Content.ReadFromJsonAsync<Response<Guid>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotEqual(Guid.Empty, result.Data);
        }

        [Fact]
        public async Task CreateRoom_WithMissingName_ReturnsBadRequest()
        {
            await AuthenticateAsAdminAsync();
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = "",
                Location = "Tầng 1"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Rooms", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors!, e => e.Contains("Tên phòng không được để trống."));
        }

        [Fact]
        public async Task GetRooms_WhenAuthenticated_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();

            HttpResponseMessage? response = await _client.GetAsync("/api/Rooms");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<IEnumerable<RoomDto>>? result = await response.Content.ReadFromJsonAsync<Response<IEnumerable<RoomDto>>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetRoomById_WithValidId_ReturnsRoomDto()
        {
            await AuthenticateAsAdminAsync();

            string? uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            CreateRoomCommand? createCmd = new CreateRoomCommand
            {
                Name = $"Test Room {uniqueSuffix}",
                Location = "Tầng Test"
            };
            HttpResponseMessage? createRes = await _client.PostAsJsonAsync("/api/Rooms", createCmd);
            Guid createdRoomId = (await createRes.Content.ReadFromJsonAsync<Response<Guid>>())!.Data;

            HttpResponseMessage? response = await _client.GetAsync($"/api/Rooms/{createdRoomId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<RoomDto>? result = await response.Content.ReadFromJsonAsync<Response<RoomDto>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(createdRoomId, result.Data!.Id);
            Assert.Equal(createCmd.Name, result.Data.Name);
        }

        [Fact]
        public async Task UpdateRoom_WithValidData_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();

            string? uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            CreateRoomCommand? createCmd = new CreateRoomCommand
            {
                Name = $"Old Room {uniqueSuffix}",
                Location = "Old Location"
            };
            HttpResponseMessage? createRes = await _client.PostAsJsonAsync("/api/Rooms", createCmd);
            Guid createdRoomId = (await createRes.Content.ReadFromJsonAsync<Response<Guid>>())!.Data;

            UpdateRoomCommand? updateCmd = new UpdateRoomCommand
            {
                Id = createdRoomId,
                Name = $"Updated Room {uniqueSuffix}",
                Location = "Updated Location"
            };

            HttpResponseMessage? response = await _client.PutAsJsonAsync($"/api/Rooms/{createdRoomId}", updateCmd);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<Guid>? result = await response.Content.ReadFromJsonAsync<Response<Guid>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(createdRoomId, result.Data);
        }

        [Fact]
        public async Task DeleteRoom_WithValidId_ReturnsOkAndDeletesRoom()
        {
            await AuthenticateAsAdminAsync();

            string? uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            CreateRoomCommand? createCmd = new CreateRoomCommand
            {
                Name = $"To Delete {uniqueSuffix}",
                Location = "Tầng Xóa"
            };
            HttpResponseMessage? createRes = await _client.PostAsJsonAsync("/api/Rooms", createCmd);
            Guid createdRoomId = (await createRes.Content.ReadFromJsonAsync<Response<Guid>>())!.Data;

            HttpResponseMessage? deleteResponse = await _client.DeleteAsync($"/api/Rooms/{createdRoomId}");

            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            HttpResponseMessage? getResponse = await _client.GetAsync($"/api/Rooms/{createdRoomId}");
            Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        }
    }
}