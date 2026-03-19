using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using ServerMonitorApp.Application.Features.Alerts.EventHandlers;
using ServerMonitorApp.Application.Features.IoT.Events;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Alerts.EventHandlers
{
    public class CheckDeviceAlertsEventHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<ILogger<CheckDeviceAlertsEventHandler>> _loggerMock;

        public CheckDeviceAlertsEventHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<CheckDeviceAlertsEventHandler>>();
        }

        [Fact]
        public async Task Handle_DeviceNotFoundOrInactive_DoesNothing()
        {
            Guid deviceId = Guid.NewGuid();
            _dbContext.Devices.Add(new Device { Id = deviceId, Name = "Inactive Sensor", IsActive = false, RoomId = Guid.NewGuid() });
            await _dbContext.SaveChangesAsync();

            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, 1L, 50m, 90m);
            CheckDeviceAlertsEventHandler handler = new CheckDeviceAlertsEventHandler(_dbContext, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            int alertCount = await _dbContext.Alerts.CountAsync();
            Assert.Equal(0, alertCount); // Không có cảnh báo nào được tạo
        }

        [Fact]
        public async Task Handle_TemperatureExceedsCritical_CreatesCriticalAlert()
        {
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Sensor 1",
                IsActive = true,
                RoomId = roomId,
                WarningTemp = 30m,
                CriticalTemp = 40m
            });
            await _dbContext.SaveChangesAsync();

            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, 1L, 45m, 50m);
            CheckDeviceAlertsEventHandler handler = new CheckDeviceAlertsEventHandler(_dbContext, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            Alert? alert = await _dbContext.Alerts.FirstOrDefaultAsync();
            Assert.NotNull(alert);
            Assert.Equal("CRITICAL", alert!.Severity);
            Assert.Contains("Nhiệt độ VƯỢT NGƯỠNG NGUY HIỂM", alert.Message);
            Assert.False(alert.IsResolved);
        }

        [Fact]
        public async Task Handle_HumidityExceedsWarning_CreatesWarningAlert()
        {
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Sensor 2",
                IsActive = true,
                RoomId = roomId,
                WarningHumidity = 60m,
                CriticalHumidity = 80m
            });
            await _dbContext.SaveChangesAsync();

            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, 1L, 25m, 70m);
            CheckDeviceAlertsEventHandler handler = new CheckDeviceAlertsEventHandler(_dbContext, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            Alert? alert = await _dbContext.Alerts.FirstOrDefaultAsync();
            Assert.NotNull(alert);
            Assert.Equal("WARNING", alert!.Severity);
            Assert.Contains("Độ ẩm cảnh báo cao", alert.Message);
        }

        [Fact]
        public async Task Handle_HasUnresolvedAlert_DoesNotCreateDuplicateAlert()
        {
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Sensor 3",
                IsActive = true,
                RoomId = roomId,
                CriticalTemp = 40m
            });

            _dbContext.Alerts.Add(new Alert
            {
                Id = 1L,
                DeviceId = deviceId,
                RoomId = roomId,
                Message = "Nhiệt độ VƯỢT NGƯỠNG NGUY HIỂM...",
                IsResolved = false
            });

            await _dbContext.SaveChangesAsync();

            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, 2L, 45m, 50m);
            CheckDeviceAlertsEventHandler handler = new CheckDeviceAlertsEventHandler(_dbContext, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            int alertCount = await _dbContext.Alerts.CountAsync();
            Assert.Equal(1, alertCount);
        }

        [Fact]
        public async Task Handle_NormalConditions_CreatesNoAlerts()
        {
            Guid deviceId = Guid.NewGuid();
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Sensor 4",
                IsActive = true,
                RoomId = Guid.NewGuid(),
                WarningTemp = 35m,
                CriticalTemp = 45m,
                WarningHumidity = 70m,
                CriticalHumidity = 90m
            });
            await _dbContext.SaveChangesAsync();

            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, 1L, 25m, 50m);
            CheckDeviceAlertsEventHandler handler = new CheckDeviceAlertsEventHandler(_dbContext, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            int alertCount = await _dbContext.Alerts.CountAsync();
            Assert.Equal(0, alertCount);
        }

        [Fact]
        public async Task Handle_DbException_CatchesAndLogsError()
        {
            DbContextOptions<ApplicationDbContext>? dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using BuggyDbContext? buggyContext = new BuggyDbContext(dbContextOptions);
            CheckDeviceAlertsEventHandler handler = new CheckDeviceAlertsEventHandler(buggyContext, _loggerMock.Object);
            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(Guid.NewGuid(), 1L, 40m, 80m);

            await handler.Handle(notification, CancellationToken.None);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        private class BuggyDbContext : ApplicationDbContext
        {
            public BuggyDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            public override DbSet<Device> Devices => throw new Exception("Database connection lost");
        }
    }
}