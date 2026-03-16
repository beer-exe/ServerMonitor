using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Features.Rooms.Queries.GetRoomById;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Queries
{
    public class GetRoomByIdQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetRoomByIdQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ExistingRoom_ReturnsRoomDto()
        {
            Guid roomId = Guid.NewGuid();
            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Server Room A", Location = "Floor 1" });
            await _dbContext.SaveChangesAsync();

            GetRoomByIdQueryHandler? handler = new GetRoomByIdQueryHandler(_dbContext);
            GetRoomByIdQuery? query = new GetRoomByIdQuery(roomId);

            Response<RoomDto>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotNull(response.Data);
            Assert.Equal(roomId, response.Data.Id);
            Assert.Equal("Server Room A", response.Data.Name);
            Assert.Equal("Floor 1", response.Data.Location);
        }

        [Fact]
        public async Task Handle_NonExistingRoom_ThrowsApiException()
        {
            GetRoomByIdQueryHandler? handler = new GetRoomByIdQueryHandler(_dbContext);
            GetRoomByIdQuery? query = new GetRoomByIdQuery(Guid.NewGuid());

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Phòng không tồn tại.", exception.Message);
        }
    }
}
