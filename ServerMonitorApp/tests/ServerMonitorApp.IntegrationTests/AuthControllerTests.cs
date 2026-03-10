using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Wrappers;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace ServerMonitorApp.IntegrationTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOkAndTokens()
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "integrationuser",
                Password = "Password123!"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", command);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string? responseString = await response.Content.ReadAsStringAsync();
            Response<AuthResponse>? result = JsonSerializer.Deserialize<Response<AuthResponse>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal("Đăng nhập thành công.", result.Message);
            Assert.NotNull(result.Data);
            Assert.NotEmpty(result.Data.AccessToken);
            Assert.NotEmpty(result.Data.RefreshToken);
            Assert.Equal("integrationuser", result.Data.FullName);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsBadRequest()
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "integrationuser",
                Password = "WrongPassword!!!"
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string? responseString = await response.Content.ReadAsStringAsync();
            Response<string>? result = JsonSerializer.Deserialize<Response<string>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Tài khoản hoặc mật khẩu không chính xác.", result.Message);
        }

        [Fact]
        public async Task Login_WithMissingFields_ReturnsBadRequest_FromValidationBehavior()
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "integrationuser",
                Password = ""
            };

            HttpResponseMessage? response = await _client.PostAsJsonAsync("/api/auth/login", command);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            string? responseString = await response.Content.ReadAsStringAsync();
            Response<string>? result = JsonSerializer.Deserialize<Response<string>>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Equal("Dữ liệu đầu vào không hợp lệ.", result.Message);
            Assert.NotNull(result.Errors);
            Assert.Contains(result.Errors, e => e.Contains("Mật khẩu không được để trống."));
        }
    }
}