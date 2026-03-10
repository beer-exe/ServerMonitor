using Microsoft.EntityFrameworkCore;
using Moq;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Auth.Commands.Login;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Auth.Commands
{
    public class LoginCommandHandlerTests
    {
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly ApplicationDbContext _dbContext;

        public LoginCommandHandlerTests()
        {
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidCredentials_ReturnsAuthResponseAndSavesRefreshToken()
        {
            User? user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                Role = "User"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "testuser",
                Password = "correct_password"
            };

            _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash)).Returns(true);
            _jwtTokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("mocked_access_token");
            _jwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken()).Returns("mocked_refresh_token");

            LoginCommandHandler? handler = new LoginCommandHandler(_dbContext, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);

            Response<AuthResponse>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Đăng nhập thành công.", response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal("mocked_access_token", response.Data.AccessToken);
            Assert.Equal("mocked_refresh_token", response.Data.RefreshToken);

            User? userInDb = await _dbContext.Users.FindAsync(user.Id);
            Assert.Equal("mocked_refresh_token", userInDb?.RefreshToken);
            Assert.NotNull(userInDb?.RefreshTokenExpiryTime);
        }

        [Fact]
        public async Task Handle_InvalidUsername_ThrowsApiException()
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "wronguser",
                Password = "any_password"
            };

            LoginCommandHandler? handler = new LoginCommandHandler(_dbContext, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Tài khoản hoặc mật khẩu không chính xác.", exception.Message);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ThrowsApiException()
        {
            User? user = new User
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hashed_password",
                Role = "User"
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "testuser",
                Password = "wrong_password"
            };

            _passwordHasherMock.Setup(x => x.VerifyPassword(command.Password, user.PasswordHash)).Returns(false);

            LoginCommandHandler? handler = new LoginCommandHandler(_dbContext, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Tài khoản hoặc mật khẩu không chính xác.", exception.Message);
        }
    }
}