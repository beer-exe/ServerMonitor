using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Features.Rooms.Queries.GetRooms;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Queries
{
    public class GetRoomsQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetRoomsQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_WithoutSearchTerm_ReturnsAllRooms()
        {
            _dbContext.Rooms.AddRange(new List<Room>
            {
                new Room { Id = Guid.NewGuid(), Name = "Server Room A", Location = "Floor 1", CreatedAt = DateTime.UtcNow },
                new Room { Id = Guid.NewGuid(), Name = "Server Room B", Location = "Floor 2", CreatedAt = DateTime.UtcNow.AddMinutes(-5) }
            });
            await _dbContext.SaveChangesAsync();

            GetRoomsQueryHandler? handler = new GetRoomsQueryHandler(_dbContext);
            GetRoomsQuery? query = new GetRoomsQuery()
            {
                Role = "ADMIN",
            };

            Response<IEnumerable<RoomDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data.Count());
            Assert.Equal("Lấy danh sách phòng thành công.", response.Message);
        }

        [Fact]
        public async Task Handle_WithSearchTerm_ReturnsFilteredRooms()
        {
            _dbContext.Rooms.AddRange(new List<Room>
            {
                new Room { Id = Guid.NewGuid(), Name = "Data Center", Location = "Building A", CreatedAt = DateTime.UtcNow },
                new Room { Id = Guid.NewGuid(), Name = "Server Room", Location = "Building B", CreatedAt = DateTime.UtcNow },
                new Room { Id = Guid.NewGuid(), Name = "Office", Location = "Building C", CreatedAt = DateTime.UtcNow }
            });
            await _dbContext.SaveChangesAsync();

            GetRoomsQueryHandler? handler = new GetRoomsQueryHandler(_dbContext);
            GetRoomsQuery? query = new GetRoomsQuery 
            { 
                Role = "ADMIN", 
                SearchTerm = "Server" 
            };

            Response<IEnumerable<RoomDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Single(response.Data);
            Assert.Equal("Server Room", response.Data.First().Name);
        }
    }
}
