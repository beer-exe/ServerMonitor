using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Features.Users.Commands.CreateUser;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Wrappers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public UsersControllerTests(CustomWebApplicationFactory factory)
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
        public async Task GetUsers_WhenAuthenticatedAsAdmin_ReturnsOk()
        {
            await AuthenticateAsAdminAsync();

            HttpResponseMessage? response = await _client.GetAsync("/api/Users");

            Console.WriteLine(await response.Content.ReadAsStringAsync());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<IEnumerable<UserDto>>? result = await response.Content.ReadFromJsonAsync<Response<IEnumerable<UserDto>>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotEmpty(result.Data!);
        }

        [Fact]
        public async Task CreateUser_WithValidData_ReturnsOkAndCreatesUser()
        {
            await AuthenticateAsAdminAsync();

            string? uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            CreateUserCommand? command = new CreateUserCommand
            {
                Username = $"newuser_{uniqueSuffix}",
                Email = $"newuser_{uniqueSuffix}@test.com",
                Password = "StrongPassword123!",
                Role = "USER"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users", command);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Response<Guid>? result = await response.Content.ReadFromJsonAsync<Response<Guid>>();
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotEqual(Guid.Empty, result.Data);
        }

        [Fact]
        public async Task CreateUser_WithInvalidEmail_ReturnsBadRequest()
        {
            await AuthenticateAsAdminAsync();

            CreateUserCommand? command = new CreateUserCommand
            {
                Username = "invalidemailuser",
                Email = "not-an-email",
                Password = "StrongPassword123!",
                Role = "USER"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/Users", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            Response<string>? result = await response.Content.ReadFromJsonAsync<Response<string>>();
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors!, e => e.Contains("Định dạng Email không hợp lệ"));
        }

        [Fact]
        public async Task GetUserById_WithValidId_ReturnsUserDto()
        {
            await AuthenticateAsAdminAsync();

            string? uniqueSuffix = Guid.NewGuid().ToString().Substring(0, 8);
            CreateUserCommand? createCmd = new CreateUserCommand
            {
                Username = $"getuser_{uniqueSuffix}",
                Email = $"getuser_{uniqueSuffix}@test.com",
                Password = "Password123!",
                Role = "USER"
            };
            HttpResponseMessage? createRes = await _client.PostAsJsonAsync("/api/Users", createCmd);
            Response<Guid>? createResult = await createRes.Content.ReadFromJsonAsync<Response<Guid>>();
            Guid createdUserId = createResult!.Data;

            HttpResponseMessage? response = await _client.GetAsync($"/api/Users/{createdUserId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Response<UserDto>? result = await response.Content.ReadFromJsonAsync<Response<UserDto>>();

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(createdUserId, result.Data!.Id);
            Assert.Equal(createCmd.Username, result.Data.Username);
        }

        [Fact]
        public async Task DeleteUser_WithValidId_ReturnsOkAndDeletesUser()
        {
            await AuthenticateAsAdminAsync();

            CreateUserCommand? createCmd = new CreateUserCommand
            {
                Username = $"todelete_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Email = $"todelete_{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                Role = "USER"
            };
            HttpResponseMessage? createRes = await _client.PostAsJsonAsync("/api/Users", createCmd);
            Guid createdUserId = (await createRes.Content.ReadFromJsonAsync<Response<Guid>>())!.Data;
            HttpResponseMessage? deleteResponse = await _client.DeleteAsync($"/api/Users/{createdUserId}");
            Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

            HttpResponseMessage? getResponse = await _client.GetAsync($"/api/Users/{createdUserId}");
            Assert.Equal(HttpStatusCode.BadRequest, getResponse.StatusCode);
        }

        [Fact]
        public async Task Endpoints_WhenNotAuthenticated_ReturnsUnauthorized()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            HttpResponseMessage? response = await _client.GetAsync("/api/Users");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
