using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Commands
{
    public class UpdateRoomAccessCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public UpdateRoomAccessCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ExistingAccess_UpdatesAndReturnsSuccess()
        {
            Guid userId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            _dbContext.UserRoomAccesses.Add(new UserRoomAccess 
            { 
                UserId = userId, 
                RoomId = roomId, 
                ReceiveAlerts = false 
            });
            await _dbContext.SaveChangesAsync();

            UpdateRoomAccessCommand? command = new UpdateRoomAccessCommand { UserId = userId, RoomId = roomId, ReceiveAlerts = true };
            UpdateRoomAccessCommandHandler? handler = new UpdateRoomAccessCommandHandler(_dbContext);

            Response<string>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Cập nhật quyền giám sát thành công.", response.Message);

            UserRoomAccess? accessInDb = await _dbContext.UserRoomAccesses.FirstOrDefaultAsync(a => a.UserId == userId && a.RoomId == roomId);
            Assert.NotNull(accessInDb);
            Assert.True(accessInDb.ReceiveAlerts);
        }

        [Fact]
        public async Task Handle_NonExistingAccess_ThrowsApiException()
        {
            UpdateRoomAccessCommand? command = new UpdateRoomAccessCommand { UserId = Guid.NewGuid(), RoomId = Guid.NewGuid(), ReceiveAlerts = true };
            UpdateRoomAccessCommandHandler? handler = new UpdateRoomAccessCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Không tìm thấy thông tin phân quyền cho người dùng và phòng này.", exception.Message);
        }
    }
}