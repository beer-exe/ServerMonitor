using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Users.Commands.DeleteUser;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Users.Commands
{
    public class DeleteUserCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public DeleteUserCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]  
        public async Task Handle_ExistingUser_DeletesUser()
        {
            Guid userId = Guid.NewGuid();
            _dbContext.Users.Add(new User { Id = userId, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", Role = "USER" });
            await _dbContext.SaveChangesAsync();

            DeleteUserCommandHandler? handler = new DeleteUserCommandHandler(_dbContext);
            DeleteUserCommand? command = new DeleteUserCommand(userId);

            Response<Guid>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(userId, response.Data);
            Assert.Equal("Xóa người dùng thành công.", response.Message);

            User? userInDb = await _dbContext.Users.FindAsync(userId);
            Assert.Null(userInDb);
        }

        [Fact]
        public async Task Handle_NonExistingUser_ThrowsApiException()
        {
            DeleteUserCommandHandler? handler = new DeleteUserCommandHandler(_dbContext);
            DeleteUserCommand? command = new DeleteUserCommand(Guid.NewGuid());

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Người dùng không tồn tại.", exception.Message);
        }
    }
}
