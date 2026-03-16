using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Features.AccessControl.Queries.GetUsersByRoomId;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Queries
{
    public class GetUsersByRoomIdQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetUsersByRoomIdQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_RoomHasUsers_ReturnsUsersList()
        {
            Guid userId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            _dbContext.Users.Add(new User 
            { 
                Id = userId, 
                Username = "manager1", 
                Email = "manager@test.com", 
                PasswordHash = "hash", Role = "USER" 
            });
            _dbContext.Rooms.Add(new Room 
            { 
                Id = roomId, 
                Name = "Data Center", 
                Location = "Basement" 
            });
            _dbContext.UserRoomAccesses.Add(new UserRoomAccess 
            { 
                UserId = userId, 
                RoomId = roomId, 
                ReceiveAlerts = false 
            });
            await _dbContext.SaveChangesAsync();

            GetUsersByRoomIdQuery? query = new GetUsersByRoomIdQuery(roomId);
            GetUsersByRoomIdQueryHandler? handler = new GetUsersByRoomIdQueryHandler(_dbContext);

            Response<IEnumerable<UserRoomAccessDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Single(response.Data);
            Assert.Equal("manager1", response.Data.First().UserName);
        }

        [Fact]
        public async Task Handle_RoomHasNoUsers_ReturnsEmptyList()
        {
            GetUsersByRoomIdQuery? query = new GetUsersByRoomIdQuery(Guid.NewGuid());
            GetUsersByRoomIdQueryHandler? handler = new GetUsersByRoomIdQueryHandler(_dbContext);

            Response<IEnumerable<UserRoomAccessDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Empty(response.Data!);
            Assert.Equal("Không tìm thấy nhân viên phụ trách quản lý nào.", response.Message);
        }
    }
}