using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Features.Users.Queries.GetUsers;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Users.Queries
{
    public class GetUsersQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetUsersQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_GetUsers_ReturnsAllUsers()
        {
            _dbContext.Users.AddRange(new List<User>
            {
                new User { Id = Guid.NewGuid(), Username = "user1", Email = "user1@test.com", PasswordHash = "hash", Role = "ADMIN" },
                new User { Id = Guid.NewGuid(), Username = "user2", Email = "user2@test.com", PasswordHash = "hash", Role = "USER" }
            });
            await _dbContext.SaveChangesAsync();

            GetUsersQueryHandler? handler = new GetUsersQueryHandler(_dbContext);
            GetUsersQuery? query = new GetUsersQuery();

            Response<IEnumerable<UserDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Lấy danh sách người dùng thành công.", response.Message);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count());
        }
    }
}
