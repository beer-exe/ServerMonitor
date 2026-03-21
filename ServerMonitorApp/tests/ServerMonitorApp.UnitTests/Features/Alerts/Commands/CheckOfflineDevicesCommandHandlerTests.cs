using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServerMonitorApp.Application.Features.Alerts.Commands.CheckOfflineDevices;
using ServerMonitorApp.Application.Features.Alerts.Events;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Alerts.Commands
{
    public class CheckOfflineDevicesCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<ILogger<CheckOfflineDevicesCommandHandler>> _loggerMock;

        public CheckOfflineDevicesCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _mediatorMock = new Mock<IMediator>();
            _loggerMock = new Mock<ILogger<CheckOfflineDevicesCommandHandler>>();
        }

        [Fact]
        public async Task Handle_OfflineDevicesExist_CreatesAlertsAndPublishesEvents()
        {
            Guid roomId = Guid.NewGuid();
            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Test Room" });

            Guid device1Id = Guid.NewGuid();
            Guid device2Id = Guid.NewGuid();
            Guid device3Id = Guid.NewGuid();
            Guid device4Id = Guid.NewGuid();

            _dbContext.Devices.AddRange(new List<Device>
            {
                new Device { Id = device1Id, RoomId = roomId, Name = "Dev 1", IsActive = true, LastSeen = DateTime.UtcNow.AddMinutes(-10) },
                new Device { Id = device2Id, RoomId = roomId, Name = "Dev 2", IsActive = true, LastSeen = null },
                new Device { Id = device3Id, RoomId = roomId, Name = "Dev 3", IsActive = true, LastSeen = DateTime.UtcNow.AddMinutes(-2) },
                new Device { Id = device4Id, RoomId = roomId, Name = "Dev 4", IsActive = false, LastSeen = DateTime.UtcNow.AddMinutes(-20) }
            });
            await _dbContext.SaveChangesAsync();

            CheckOfflineDevicesCommand command = new CheckOfflineDevicesCommand();
            CheckOfflineDevicesCommandHandler handler = new CheckOfflineDevicesCommandHandler(_dbContext, _mediatorMock.Object, _loggerMock.Object);

            int alertsCreatedCount = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(2, alertsCreatedCount);

            List<Alert> newAlerts = await _dbContext.Alerts.ToListAsync();
            Assert.Equal(2, newAlerts.Count);
            Assert.All(newAlerts, a =>
            {
                Assert.Equal("OFFLINE", a.Severity);
                Assert.False(a.IsResolved);
            });
            Assert.Contains(newAlerts, a => a.DeviceId == device1Id);
            Assert.Contains(newAlerts, a => a.DeviceId == device2Id);

            _mediatorMock.Verify(m => m.Publish(
                It.Is<DeviceAlertTriggeredEvent>(e => e.Alert.Severity == "OFFLINE"),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_UnresolvedOfflineAlertAlreadyExists_DoesNotCreateDuplicateAlert()
        {
            Guid roomId = Guid.NewGuid();
            Guid deviceId = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Test Room" });
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                RoomId = roomId,
                Name = "Dev 1",
                IsActive = true,
                LastSeen = DateTime.UtcNow.AddMinutes(-10)
            });

            _dbContext.Alerts.Add(new Alert
            {
                DeviceId = deviceId,
                RoomId = roomId,
                Message = "Thiết bị mất kết nối",
                Severity = "OFFLINE",
                IsResolved = false
            });
            await _dbContext.SaveChangesAsync();

            CheckOfflineDevicesCommand command = new CheckOfflineDevicesCommand();
            CheckOfflineDevicesCommandHandler handler = new CheckOfflineDevicesCommandHandler(_dbContext, _mediatorMock.Object, _loggerMock.Object);

            int alertsCreatedCount = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(0, alertsCreatedCount);

            int totalAlerts = await _dbContext.Alerts.CountAsync();
            Assert.Equal(1, totalAlerts);

            _mediatorMock.Verify(m => m.Publish(It.IsAny<DeviceAlertTriggeredEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ResolvedOfflineAlertExists_CreatesNewAlert()
        {
            Guid roomId = Guid.NewGuid();
            Guid deviceId = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Test Room" });
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                RoomId = roomId,
                Name = "Dev 1",
                IsActive = true,
                LastSeen = DateTime.UtcNow.AddMinutes(-15)
            });

            _dbContext.Alerts.Add(new Alert
            {
                DeviceId = deviceId,
                RoomId = roomId,
                Message = "Thiết bị mất kết nối (lần trước)",
                Severity = "OFFLINE",
                IsResolved = true
            });
            await _dbContext.SaveChangesAsync();

            CheckOfflineDevicesCommand command = new CheckOfflineDevicesCommand();
            CheckOfflineDevicesCommandHandler handler = new CheckOfflineDevicesCommandHandler(_dbContext, _mediatorMock.Object, _loggerMock.Object);

            int alertsCreatedCount = await handler.Handle(command, CancellationToken.None);

            Assert.Equal(1, alertsCreatedCount);

            int totalAlerts = await _dbContext.Alerts.CountAsync();
            Assert.Equal(2, totalAlerts);

            _mediatorMock.Verify(m => m.Publish(It.IsAny<DeviceAlertTriggeredEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}