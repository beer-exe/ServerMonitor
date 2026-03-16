using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Rooms.Commands.UpdateRoom;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Commands
{
    public class UpdateRoomCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public UpdateRoomCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidData_UpdatesRoomAndReturnsGuid()
        {
            Guid roomId = Guid.NewGuid();
            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Old Name", Location = "Old Location" });
            await _dbContext.SaveChangesAsync();

            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = roomId,
                Name = "New Name",
                Location = "New Location"
            };

            UpdateRoomCommandHandler? handler = new UpdateRoomCommandHandler(_dbContext);

            Response<Guid> response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(roomId, response.Data);
            Assert.Equal("Cập nhật thông tin phòng thành công.", response.Message);

            Room? updatedRoom = await _dbContext.Rooms.FindAsync(roomId);
            Assert.Equal("New Name", updatedRoom?.Name);
            Assert.Equal("New Location", updatedRoom?.Location);
            Assert.NotNull(updatedRoom?.UpdatedAt);
        }

        [Fact]
        public async Task Handle_RoomNotFound_ThrowsApiException()
        {
            UpdateRoomCommand? command = new UpdateRoomCommand { Id = Guid.NewGuid(), Name = "Name", Location = "Location" };
            UpdateRoomCommandHandler? handler = new UpdateRoomCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Phòng không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_DuplicateNameAndLocation_ThrowsApiException()
        {
            Guid room1Id = Guid.NewGuid();
            Guid room2Id = Guid.NewGuid();
            _dbContext.Rooms.AddRange(new List<Room>
            {
                new Room { Id = room1Id, Name = "Room 1", Location = "Floor 1" },
                new Room { Id = room2Id, Name = "Room 2", Location = "Floor 2" }
            });
            await _dbContext.SaveChangesAsync();

            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = room1Id,
                Name = "Room 2",
                Location = "Floor 2"
            };
            UpdateRoomCommandHandler? handler = new UpdateRoomCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Đã tồn tại phòng có tên 'Room 2' tại vị trí 'Floor 2'. Vui lòng chọn tên hoặc vị trí khác.", exception.Message);
        }
    }
}
