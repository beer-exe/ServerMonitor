using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Users.Commands.UpdateUser;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Users.Commands
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public UpdateUserCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidData_UpdatesUserAndReturnsGuid()
        {
            Guid userId = Guid.NewGuid();
            _dbContext.Users.Add(new User { Id = userId, Username = "testuser", Email = "old@test.com", PasswordHash = "hash", Role = "USER" });
            await _dbContext.SaveChangesAsync();

            UpdateUserCommand? command = new UpdateUserCommand
            {
                Id = userId,
                Email = "updated@test.com",
                Role = "ADMIN"
            };

            UpdateUserCommandHandler? handler = new UpdateUserCommandHandler(_dbContext);

            Response<Guid>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(userId, response.Data);

            User? updatedUser = await _dbContext.Users.FindAsync(userId);
            Assert.Equal("updated@test.com", updatedUser?.Email);
            Assert.Equal("ADMIN", updatedUser?.Role);
            Assert.NotNull(updatedUser?.UpdatedAt);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsApiException()
        {
            UpdateUserCommand? command = new UpdateUserCommand { Id = Guid.NewGuid(), Email = "test@test.com", Role = "USER" };
            UpdateUserCommandHandler? handler = new UpdateUserCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Người dùng không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_EmailAlreadyUsedByAnotherUser_ThrowsApiException()
        {
            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            _dbContext.Users.AddRange(new List<User>
            {
                new User { Id = user1Id, Username = "user1", Email = "user1@test.com", PasswordHash = "hash", Role = "USER" },
                new User { Id = user2Id, Username = "user2", Email = "user2@test.com", PasswordHash = "hash", Role = "USER" }
            });
            await _dbContext.SaveChangesAsync();

            UpdateUserCommand? command = new UpdateUserCommand
            {
                Id = user1Id,
                Email = "user2@test.com",
                Role = "ADMIN"
            };

            UpdateUserCommandHandler? handler = new UpdateUserCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Email 'user2@test.com' đã được sử dụng bởi một tài khoản khác.", exception.Message);
        }
    }
}
