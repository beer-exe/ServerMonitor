using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Features.Users.Queries.GetUserById;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Users.Queries
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetUserByIdQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ExistingUser_ReturnsUserDto()
        {
            Guid userId = Guid.NewGuid();
            User? user = new User { Id = userId, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", Role = "USER" };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            GetUserByIdQueryHandler? handler = new GetUserByIdQueryHandler(_dbContext);
            GetUserByIdQuery? query = new GetUserByIdQuery(userId);

            Response<UserDto>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Equal(userId, response.Data.Id);
            Assert.Equal("testuser", response.Data.Username);
        }

        [Fact]
        public async Task Handle_NonExistingUser_ThrowsApiException()
        {
            GetUserByIdQueryHandler? handler = new GetUserByIdQueryHandler(_dbContext);
            GetUserByIdQuery? query = new GetUserByIdQuery(Guid.NewGuid());

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Người dùng không tồn tại.", exception.Message);
        }
    }
}
