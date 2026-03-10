using Microsoft.EntityFrameworkCore;
using Moq;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Auth.Commands.RefreshToken;
using ServerMonitorApp.Application.Features.Auth.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;
using System.Security.Claims;

namespace ServerMonitorApp.UnitTests.Features.Auth.Commands
{
    public class RefreshTokenCommandHandlerTests
    {
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
        private readonly ApplicationDbContext _dbContext;

        public RefreshTokenCommandHandlerTests()
        {
            _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidTokens_ReturnsNewTokens()
        {
            Guid userId = Guid.NewGuid();
            User? user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User",
                RefreshToken = "valid_old_refresh_token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            RefreshTokenCommand? command = new RefreshTokenCommand
            {
                AccessToken = "valid_old_access_token",
                RefreshToken = "valid_old_refresh_token"
            };

            List<Claim>? claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            ClaimsIdentity? identity = new ClaimsIdentity(claims);
            ClaimsPrincipal? principal = new ClaimsPrincipal(identity);

            _jwtTokenGeneratorMock.Setup(x => x.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(principal);
            _jwtTokenGeneratorMock.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("new_access_token");
            _jwtTokenGeneratorMock.Setup(x => x.GenerateRefreshToken()).Returns("new_refresh_token");

            RefreshTokenCommandHandler? handler = new RefreshTokenCommandHandler(_dbContext, _jwtTokenGeneratorMock.Object);

            Response<AuthResponse>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Refresh Token thành công.", response.Message);
            Assert.Equal("new_access_token", response.Data?.AccessToken);
            Assert.Equal("new_refresh_token", response.Data?.RefreshToken);

            User? userInDb = await _dbContext.Users.FindAsync(userId);
            Assert.Equal("new_refresh_token", userInDb?.RefreshToken);
            Assert.True(userInDb?.RefreshTokenExpiryTime > DateTime.UtcNow);
        }

        [Fact]
        public async Task Handle_InvalidAccessToken_ThrowsApiException()
        {
            RefreshTokenCommand? command = new RefreshTokenCommand
            {
                AccessToken = "invalid_access_token",
                RefreshToken = "any_refresh_token"
            };

            ClaimsPrincipal? principal = new ClaimsPrincipal(new ClaimsIdentity());
            _jwtTokenGeneratorMock.Setup(x => x.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(principal);

            RefreshTokenCommandHandler? handler = new RefreshTokenCommandHandler(_dbContext, _jwtTokenGeneratorMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Access Token không hợp lệ.", exception.Message);
        }

        [Fact]
        public async Task Handle_ExpiredRefreshToken_ThrowsApiException()
        {
            Guid userId = Guid.NewGuid();
            User? user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User",
                RefreshToken = "old_refresh_token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            RefreshTokenCommand? command = new RefreshTokenCommand
            {
                AccessToken = "valid_old_access_token",
                RefreshToken = "old_refresh_token"
            };

            List<Claim>? claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            ClaimsPrincipal? principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _jwtTokenGeneratorMock.Setup(x => x.GetPrincipalFromExpiredToken(command.AccessToken))
                .Returns(principal);

            RefreshTokenCommandHandler? handler = new RefreshTokenCommandHandler(_dbContext, _jwtTokenGeneratorMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Refresh Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.", exception.Message);
        }

        [Fact]
        public async Task Handle_MismatchedRefreshToken_ThrowsApiException()
        {
            Guid userId = Guid.NewGuid();
            User? user = new User
            {
                Id = userId,
                Username = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = "User",
                RefreshToken = "correct_refresh_token",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
            };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            RefreshTokenCommand? command = new RefreshTokenCommand
            {
                AccessToken = "valid_old_access_token",
                RefreshToken = "wrong_refresh_token_from_hacker"
            };

            List<Claim>? claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
            ClaimsPrincipal? principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

            _jwtTokenGeneratorMock.Setup(x => x.GetPrincipalFromExpiredToken(command.AccessToken)).Returns(principal);

            RefreshTokenCommandHandler? handler = new RefreshTokenCommandHandler(_dbContext, _jwtTokenGeneratorMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Refresh Token không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.", exception.Message);
        }
    }
}