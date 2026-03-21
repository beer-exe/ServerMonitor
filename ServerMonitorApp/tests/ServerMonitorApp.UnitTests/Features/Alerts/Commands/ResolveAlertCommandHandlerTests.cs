using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Alerts.Commands
{
    public class ResolveAlertCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public ResolveAlertCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidData_UpdatesAlertAndReturnsId()
        {
            long alertId = 1;
            Guid roomId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            _dbContext.Alerts.Add(new Alert
            {
                Id = alertId,
                RoomId = roomId,
                Message = "Cảnh báo nhiệt độ cao",
                IsResolved = false,
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            });
            await _dbContext.SaveChangesAsync();

            ResolveAlertCommand command = new ResolveAlertCommand
            {
                Id = alertId,
                UserId = userId,
                Role = "ADMIN",
                ResolutionNote = "Đã khởi động lại máy lạnh."
            };

            ResolveAlertCommandHandler handler = new ResolveAlertCommandHandler(_dbContext);

            Response<long> response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(alertId, response.Data);
            Assert.Equal("Đã cập nhật trạng thái xử lý sự cố thành công.", response.Message);

            Alert? updatedAlert = await _dbContext.Alerts.FindAsync(alertId);
            Assert.NotNull(updatedAlert);
            Assert.True(updatedAlert!.IsResolved);
            Assert.Contains("\n[Đã xử lý]: Đã khởi động lại máy lạnh.", updatedAlert.Message);
            Assert.True(updatedAlert.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
        }

        [Fact]
        public async Task Handle_AlertNotFound_ThrowsApiException()
        {
            ResolveAlertCommand command = new ResolveAlertCommand
            {
                Id = 999,
                UserId = Guid.NewGuid(),
                Role = "ADMIN",
                ResolutionNote = "Test note"
            };

            ResolveAlertCommandHandler handler = new ResolveAlertCommandHandler(_dbContext);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Không tìm thấy cảnh báo.", exception.Message);
        }

        [Fact]
        public async Task Handle_UserWithoutAccess_ThrowsUnauthorizedAccessException()
        {
            long alertId = 2;
            Guid roomId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            _dbContext.Alerts.Add(new Alert
            {
                Id = alertId,
                RoomId = roomId,
                Message = "Cảnh báo ngập nước",
                IsResolved = false
            });
            await _dbContext.SaveChangesAsync();

            ResolveAlertCommand command = new ResolveAlertCommand
            {
                Id = alertId,
                UserId = userId,
                Role = "USER",
                ResolutionNote = "Đã lau dọn"
            };

            ResolveAlertCommandHandler handler = new ResolveAlertCommandHandler(_dbContext);

            UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Bạn không có quyền xử lý cảnh báo của phòng này.", exception.Message);
        }

        [Fact]
        public async Task Handle_AlertAlreadyResolved_ThrowsApiException()
        {
            long alertId = 3;
            Guid roomId = Guid.NewGuid();

            _dbContext.Alerts.Add(new Alert
            {
                Id = alertId,
                RoomId = roomId,
                Message = "Cảnh báo cháy",
                IsResolved = true
            });
            await _dbContext.SaveChangesAsync();

            ResolveAlertCommand command = new ResolveAlertCommand
            {
                Id = alertId,
                UserId = Guid.NewGuid(),
                Role = "ADMIN",
                ResolutionNote = "Check lại lần nữa"
            };

            ResolveAlertCommandHandler handler = new ResolveAlertCommandHandler(_dbContext);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Cảnh báo này đã được xử lý trước đó.", exception.Message);
        }
    }
}