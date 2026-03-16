using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.AccessControl.Commands.AssignRoomAccess;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Commands
{
    public class AssignRoomAccessCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public AssignRoomAccessCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidData_AssignsAccessAndReturnsSuccess()
        {
            Guid userId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            _dbContext.Users.Add(new User 
            { 
                Id = userId, 
                Username = "testuser", 
                Email = "test@test.com", 
                PasswordHash = "hash", 
                Role = "USER" 
            });
            _dbContext.Rooms.Add(new Room 
            { 
                Id = roomId,
                Name = "Server Room 1",
                Location = "Floor 1" 
            });
            await _dbContext.SaveChangesAsync();

            AssignRoomAccessCommand? command = new AssignRoomAccessCommand { UserId = userId, RoomId = roomId, ReceiveAlerts = true };
            AssignRoomAccessCommandHandler? handler = new AssignRoomAccessCommandHandler(_dbContext);

            Response<string>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Cấp quyền giám sát phòng thành công.", response.Message);

            UserRoomAccess? accessInDb = await _dbContext.UserRoomAccesses.FirstOrDefaultAsync(a => a.UserId == userId && a.RoomId == roomId);
            Assert.NotNull(accessInDb);
            Assert.True(accessInDb.ReceiveAlerts);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsApiException()
        {
            AssignRoomAccessCommand? command = new AssignRoomAccessCommand { UserId = Guid.NewGuid(), RoomId = Guid.NewGuid(), ReceiveAlerts = true };
            AssignRoomAccessCommandHandler? handler = new AssignRoomAccessCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Mã nhân viên không tồn tại không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_RoomNotFound_ThrowsApiException()
        {
            Guid userId = Guid.NewGuid();
            _dbContext.Users.Add(new User { Id = userId, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", Role = "USER" });
            await _dbContext.SaveChangesAsync();

            AssignRoomAccessCommand? command = new AssignRoomAccessCommand { UserId = userId, RoomId = Guid.NewGuid(), ReceiveAlerts = true };
            AssignRoomAccessCommandHandler? handler = new AssignRoomAccessCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Mã phòng không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_AccessAlreadyExists_ThrowsApiException()
        {
            Guid userId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            _dbContext.Users.Add(new User 
            { 
                Id = userId,
                Username = "testuser", 
                Email = "test@test.com", 
                PasswordHash = "hash", 
                Role = "USER"
            });
            _dbContext.Rooms.Add(new Room
            {
                Id = roomId,
                Name = "Server Room 1", 
                Location = "Floor 1" 
            });
            _dbContext.UserRoomAccesses.Add(new UserRoomAccess 
            { 
                UserId = userId, 
                RoomId = roomId, 
                ReceiveAlerts = true 
            });

            await _dbContext.SaveChangesAsync();

            AssignRoomAccessCommand? command = new AssignRoomAccessCommand { UserId = userId, RoomId = roomId, ReceiveAlerts = true };
            AssignRoomAccessCommandHandler? handler = new AssignRoomAccessCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Nhân viên này đã được phân quyền quản lý phòng này trước đó.", exception.Message);
        }
    }
}