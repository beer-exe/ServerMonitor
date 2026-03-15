using Microsoft.EntityFrameworkCore;
using Moq;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Users.Commands.CreateUser;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Users.Commands
{
    public class CreateUserCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;

        public CreateUserCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _passwordHasherMock = new Mock<IPasswordHasher>();
        }

        [Fact]
        public async Task Handle_ValidData_CreatesUserAndReturnsGuid()
        {
            CreateUserCommand? command = new CreateUserCommand
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "Password123",
                Role = "USER"
            };

            _passwordHasherMock.Setup(x => x.HashPassword(command.Password)).Returns("hashed_password");

            CreateUserCommandHandler? handler = new CreateUserCommandHandler(_dbContext, _passwordHasherMock.Object);

            Response<Guid>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotEqual(Guid.Empty, response.Data);
            Assert.Equal("Tạo người dùng thành công.", response.Message);

            User? userInDb = await _dbContext.Users.FindAsync(response.Data);
            Assert.NotNull(userInDb);
            Assert.Equal("newuser", userInDb.Username);
            Assert.Equal("hashed_password", userInDb.PasswordHash);
        }

        [Fact]
        public async Task Handle_DuplicateUsername_ThrowsApiException()
        {
            _dbContext.Users.Add(new User { Id = Guid.NewGuid(), Username = "duplicate", Email = "user1@test.com", PasswordHash = "hash", Role = "USER" });
            await _dbContext.SaveChangesAsync();

            CreateUserCommand? command = new CreateUserCommand { Username = "duplicate", Email = "new@test.com", Password = "pw", Role = "USER" };
            CreateUserCommandHandler? handler = new CreateUserCommandHandler(_dbContext, _passwordHasherMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal($"Tên đăng nhập 'duplicate' đã tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ThrowsApiException()
        {
            _dbContext.Users.Add(new User { Id = Guid.NewGuid(), Username = "user1", Email = "duplicate@test.com", PasswordHash = "hash", Role = "USER" });
            await _dbContext.SaveChangesAsync();

            CreateUserCommand? command = new CreateUserCommand { Username = "newuser", Email = "duplicate@test.com", Password = "pw", Role = "USER" };
            CreateUserCommandHandler? handler = new CreateUserCommandHandler(_dbContext, _passwordHasherMock.Object);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal($"Email 'duplicate@test.com' đã được sử dụng.", exception.Message);
        }
    }
}
