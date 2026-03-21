using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Alerts.Queries.GetAlertById;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Alerts.Queries
{
    public class GetAlertByIdQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetAlertByIdQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsAlertDto()
        {
            long alertId = 1;
            Guid roomId = Guid.NewGuid();
            Guid deviceId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Phòng Server 1" });
            _dbContext.Devices.Add(new Device { Id = deviceId, RoomId = roomId, Name = "Cảm biến nhiệt" });

            _dbContext.UserRoomAccesses.Add(new UserRoomAccess { UserId = userId, RoomId = roomId, ReceiveAlerts = true });

            _dbContext.Alerts.Add(new Alert
            {
                Id = alertId,
                RoomId = roomId,
                DeviceId = deviceId,
                Message = "Nhiệt độ quá cao",
                Severity = "CRITICAL",
                IsResolved = false
            });
            await _dbContext.SaveChangesAsync();

            GetAlertByIdQuery query = new GetAlertByIdQuery
            {
                Id = alertId,
                UserId = userId,
                Role = "USER"
            };

            GetAlertByIdQueryHandler handler = new GetAlertByIdQueryHandler(_dbContext);

            Response<AlertDto> response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Lấy thông tin cảnh báo thành công.", response.Message);
            Assert.NotNull(response.Data);

            Assert.Equal(alertId, response.Data.Id);
            Assert.Equal(roomId, response.Data.RoomId);
            Assert.Equal("Phòng Server 1", response.Data.RoomName);
            Assert.Equal(deviceId, response.Data.DeviceId);
            Assert.Equal("Cảm biến nhiệt", response.Data.DeviceName);
            Assert.Equal("CRITICAL", response.Data.Severity);
            Assert.False(response.Data.IsResolved);
        }

        [Fact]
        public async Task Handle_AlertNotFound_ThrowsApiException()
        {
            GetAlertByIdQuery query = new GetAlertByIdQuery
            {
                Id = 999,
                UserId = Guid.NewGuid(),
                Role = "ADMIN"
            };

            GetAlertByIdQueryHandler handler = new GetAlertByIdQueryHandler(_dbContext);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Không tìm thấy cảnh báo.", exception.Message);
        }

        [Fact]
        public async Task Handle_UserWithoutAccess_ThrowsUnauthorizedAccessException()
        {
            long alertId = 2;
            Guid roomId = Guid.NewGuid();
            Guid unassignedUserId = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Phòng Mạng" });
            _dbContext.Alerts.Add(new Alert
            {
                Id = alertId,
                RoomId = roomId,
                Message = "Mất kết nối",
                Severity = "OFFLINE",
                IsResolved = false
            });
            await _dbContext.SaveChangesAsync();

            GetAlertByIdQuery query = new GetAlertByIdQuery
            {
                Id = alertId,
                UserId = unassignedUserId,
                Role = "USER"
            };

            GetAlertByIdQueryHandler handler = new GetAlertByIdQueryHandler(_dbContext);

            UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Bạn không có quyền xem cảnh báo của phòng này.", exception.Message);
        }
    }
}