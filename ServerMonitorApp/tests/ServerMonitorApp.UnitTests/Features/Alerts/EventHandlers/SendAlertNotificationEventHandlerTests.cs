using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Alerts.EventHandlers;
using ServerMonitorApp.Application.Features.Alerts.Events;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Alerts.EventHandlers
{
    public class SendAlertNotificationEventHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IMonitorHubDispatcher> _hubDispatcherMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<SendAlertNotificationEventHandler>> _loggerMock;

        public SendAlertNotificationEventHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _hubDispatcherMock = new Mock<IMonitorHubDispatcher>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<SendAlertNotificationEventHandler>>();
        }

        [Fact]
        public async Task Handle_ValidEvent_SendsSignalRAndEmailsToCorrectUsers()
        {
            Guid roomId = Guid.NewGuid();
            Guid deviceId = Guid.NewGuid();

            Guid user1Id = Guid.NewGuid();
            Guid user2Id = Guid.NewGuid();
            Guid user3Id = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Phòng Server 1" });
            _dbContext.Rooms.Add(new Room { Id = Guid.NewGuid(), Name = "Phòng Server 2" });

            _dbContext.Users.AddRange(new List<User>
            {
                new User { Id = user1Id, Username = "user1", Email = "user1@test.com", PasswordHash = "hash", Role = "USER" },
                new User { Id = user2Id, Username = "user2", Email = "user2@test.com", PasswordHash = "hash", Role = "USER" },
                new User { Id = user3Id, Username = "user3", Email = "user3@test.com", PasswordHash = "hash", Role = "USER" }
            });

            _dbContext.UserRoomAccesses.AddRange(new List<UserRoomAccess>
            {
                new UserRoomAccess { UserId = user1Id, RoomId = roomId, ReceiveAlerts = true },
                new UserRoomAccess { UserId = user2Id, RoomId = roomId, ReceiveAlerts = false },
                new UserRoomAccess { UserId = user3Id, RoomId = Guid.NewGuid(), ReceiveAlerts = true }
            });

            await _dbContext.SaveChangesAsync();

            Alert alert = new Alert
            {
                Id = 1,
                RoomId = roomId,
                DeviceId = deviceId,
                Message = "Nhiệt độ phòng quá cao!",
                Severity = "CRITICAL",
                CreatedAt = DateTime.UtcNow
            };

            DeviceAlertTriggeredEvent notification = new DeviceAlertTriggeredEvent(alert);
            SendAlertNotificationEventHandler handler = new SendAlertNotificationEventHandler(
                _hubDispatcherMock.Object,
                _emailServiceMock.Object,
                _dbContext,
                _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            string expectedGroupName = $"Room_{roomId}";
            _hubDispatcherMock.Verify(h => h.SendAlertToGroupAsync(
                expectedGroupName,
                It.Is<AlertDto>(a => a.Id == alert.Id && a.Message == alert.Message)
            ), Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync(
                "user1@test.com",
                It.IsAny<string>(),
                It.IsAny<string>(),
                true
            ), Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync("user2@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            _emailServiceMock.Verify(e => e.SendAsync("user3@test.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task Handle_SignalRException_LogsErrorAndContinuesToEmail()
        {
            Guid roomId = Guid.NewGuid();
            Guid user1Id = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Phòng Server" });
            _dbContext.Users.Add(new User { Id = user1Id, Username = "user1", Email = "user1@test.com", PasswordHash = "hash", Role = "USER" });
            _dbContext.UserRoomAccesses.Add(new UserRoomAccess { UserId = user1Id, RoomId = roomId, ReceiveAlerts = true });
            await _dbContext.SaveChangesAsync();

            Alert alert = new Alert { Id = 2, RoomId = roomId, Severity = "WARNING", Message = "Cảnh báo" };
            DeviceAlertTriggeredEvent notification = new DeviceAlertTriggeredEvent(alert);

            _hubDispatcherMock
                .Setup(h => h.SendAlertToGroupAsync(It.IsAny<string>(), It.IsAny<AlertDto>()))
                .ThrowsAsync(new Exception("SignalR connection timeout"));

            SendAlertNotificationEventHandler handler = new SendAlertNotificationEventHandler(
                _hubDispatcherMock.Object,
                _emailServiceMock.Object,
                _dbContext,
                _loggerMock.Object);

            Exception? caughtException = await Record.ExceptionAsync(() => handler.Handle(notification, CancellationToken.None));

            Assert.Null(caughtException);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);

            _emailServiceMock.Verify(e => e.SendAsync("user1@test.com", It.IsAny<string>(), It.IsAny<string>(), true), Times.Once);
        }

        [Fact]
        public async Task Handle_EmailException_LogsErrorAndDoesNotCrash()
        {
            Guid roomId = Guid.NewGuid();
            Guid user1Id = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Phòng Server" });
            _dbContext.Users.Add(new User { Id = user1Id, Username = "user1", Email = "user1@test.com", PasswordHash = "hash", Role = "USER" });
            _dbContext.UserRoomAccesses.Add(new UserRoomAccess { UserId = user1Id, RoomId = roomId, ReceiveAlerts = true });
            await _dbContext.SaveChangesAsync();

            Alert alert = new Alert { Id = 3, RoomId = roomId, Severity = "CRITICAL", Message = "Mất điện" };
            DeviceAlertTriggeredEvent notification = new DeviceAlertTriggeredEvent(alert);

            _emailServiceMock
                .Setup(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ThrowsAsync(new Exception("SMTP server unavailable"));

            SendAlertNotificationEventHandler handler = new SendAlertNotificationEventHandler(
                _hubDispatcherMock.Object,
                _emailServiceMock.Object,
                _dbContext,
                _loggerMock.Object);

            Exception? caughtException = await Record.ExceptionAsync(() => handler.Handle(notification, CancellationToken.None));

            Assert.Null(caughtException);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}