using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Rooms.Commands.DeleteRoom;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Commands
{
    public class DeleteRoomCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public DeleteRoomCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ExistingRoomWithoutUnresolvedAlerts_DeletesRoom()
        {
            Guid roomId = Guid.NewGuid();
            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Server Room A", Location = "Floor 1" });
            await _dbContext.SaveChangesAsync();

            DeleteRoomCommandHandler? handler = new DeleteRoomCommandHandler(_dbContext);
            DeleteRoomCommand? command = new DeleteRoomCommand(roomId);

            Response<Guid>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(roomId, response.Data);
            Assert.Equal("Xóa phòng thành công.", response.Message);

            Room? roomInDb = await _dbContext.Rooms.FindAsync(roomId);
            Assert.Null(roomInDb);
        }

        [Fact]
        public async Task Handle_NonExistingRoom_ThrowsApiException()
        {
            DeleteRoomCommandHandler? handler = new DeleteRoomCommandHandler(_dbContext);
            DeleteRoomCommand? command = new DeleteRoomCommand(Guid.NewGuid());

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Phòng không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_RoomWithUnresolvedAlerts_ThrowsApiException()
        {
            Guid roomId = Guid.NewGuid();
            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Server Room A", Location = "Floor 1" });
            _dbContext.Alerts.Add(new Alert { Id = 1, RoomId = roomId, Message = "High Temp", IsResolved = false });
            await _dbContext.SaveChangesAsync();

            DeleteRoomCommandHandler? handler = new DeleteRoomCommandHandler(_dbContext);
            DeleteRoomCommand? command = new DeleteRoomCommand(roomId);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Không thể xóa phòng đang có cảnh báo chưa được xử lý. Vui lòng xử lý tất cả cảnh báo trước.", exception.Message);
        }
    }
}
