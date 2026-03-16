using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.AccessControl.Commands.RevokeRoomAccess;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Commands
{
    public class RevokeRoomAccessCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public RevokeRoomAccessCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ExistingAccess_RevokesAccessAndReturnsSuccess()
        {
            Guid userId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            _dbContext.UserRoomAccesses.Add(new UserRoomAccess 
            {
                UserId = userId, 
                RoomId = roomId, 
                ReceiveAlerts = true 
            });

            await _dbContext.SaveChangesAsync();

            RevokeRoomAccessCommand? command = new RevokeRoomAccessCommand(userId, roomId);
            RevokeRoomAccessCommandHandler? handler = new RevokeRoomAccessCommandHandler(_dbContext);

            Response<string>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Đã thu hồi quyền giám sát phòng của người dùng.", response.Message);

            UserRoomAccess? accessInDb = await _dbContext.UserRoomAccesses.FirstOrDefaultAsync(a => a.UserId == userId && a.RoomId == roomId);
            Assert.Null(accessInDb);
        }

        [Fact]
        public async Task Handle_NonExistingAccess_ThrowsApiException()
        {
            RevokeRoomAccessCommand? command = new RevokeRoomAccessCommand(Guid.NewGuid(), Guid.NewGuid());
            RevokeRoomAccessCommandHandler? handler = new RevokeRoomAccessCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Không tìm thấy thông tin phân quyền để thu hồi.", exception.Message);
        }
    }
}