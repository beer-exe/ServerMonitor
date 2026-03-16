using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Features.AccessControl.Queries.GetRoomsByUserId;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Queries
{
    public class GetRoomsByUserIdQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetRoomsByUserIdQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_UserHasRooms_ReturnsRoomsList()
        {
            Guid userId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            _dbContext.Users.Add(new User 
            { 
                Id = userId, 
                Username = "admin1", 
                Email = "admin1@test.com", 
                PasswordHash = "hash", 
                Role = "ADMIN" 
            });
            _dbContext.Rooms.Add(new Room 
            { 
                Id = roomId, 
                Name = "Server Room A",
                Location = "Floor 1"
            });
            _dbContext.UserRoomAccesses.Add(new UserRoomAccess 
            { 
                UserId = userId, 
                RoomId = roomId, 
                ReceiveAlerts = true 
            });
            await _dbContext.SaveChangesAsync();

            GetRoomsByUserIdQuery? query = new GetRoomsByUserIdQuery(userId);
            GetRoomsByUserIdQueryHandler? handler = new GetRoomsByUserIdQueryHandler(_dbContext);

            Response<IEnumerable<UserRoomAccessDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Single(response.Data);
            Assert.Equal("Server Room A", response.Data.First().RoomName);
            Assert.Equal(true, response.Data.First().ReceiveAlerts);
        }

        [Fact]
        public async Task Handle_UserHasNoRooms_ReturnsEmptyList()
        {
            GetRoomsByUserIdQuery? query = new GetRoomsByUserIdQuery(Guid.NewGuid());
            GetRoomsByUserIdQueryHandler? handler = new GetRoomsByUserIdQueryHandler(_dbContext);

            Response<IEnumerable<UserRoomAccessDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Empty(response.Data!);
            Assert.Equal("Không tìm thấy phòng nào được phân quyền cho nhân viên này.", response.Message);
        }
    }
}