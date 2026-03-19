using Microsoft.Extensions.Logging;
using Moq;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Dashboard.EventHandlers;
using ServerMonitorApp.Application.Features.IoT.Events;


namespace ServerMonitorApp.UnitTests.Features.Dashboard.EventHandlers
{
    public class SendRealtimeUpdateEventHandlerTests
    {
        private readonly Mock<IMonitorHubService> _monitorHubServiceMock;
        private readonly Mock<ILogger<SendRealtimeUpdateEventHandler>> _loggerMock;

        public SendRealtimeUpdateEventHandlerTests()
        {
            _monitorHubServiceMock = new Mock<IMonitorHubService>();
            _loggerMock = new Mock<ILogger<SendRealtimeUpdateEventHandler>>();
        }

        [Fact]
        public async Task Handle_ShouldCallSendDeviceUpdateAsync_WithCorrectParameters()
        {
            Guid deviceId = Guid.NewGuid();
            long sensorDataId = 1L;
            decimal temperature = 25.5m;
            decimal humidity = 60.0m;

            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, sensorDataId, temperature, humidity);
            SendRealtimeUpdateEventHandler handler = new SendRealtimeUpdateEventHandler(_monitorHubServiceMock.Object, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            _monitorHubServiceMock.Verify(x => x.SendDeviceUpdateAsync(deviceId, temperature, humidity), Times.Once);
        }

        [Fact]
        public async Task Handle_WhenServiceThrowsException_ShouldCatchAndLogWarning()
        {
            Guid deviceId = Guid.NewGuid();
            SensorDataRecordedEvent notification = new SensorDataRecordedEvent(deviceId, 1L, 25.5m, 60.0m);
            Exception exception = new Exception("SignalR connection failed");

            _monitorHubServiceMock.Setup(x => x.SendDeviceUpdateAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>())).ThrowsAsync(exception);

            SendRealtimeUpdateEventHandler handler = new SendRealtimeUpdateEventHandler(_monitorHubServiceMock.Object, _loggerMock.Object);

            await handler.Handle(notification, CancellationToken.None);

            _monitorHubServiceMock.Verify(x => x.SendDeviceUpdateAsync(deviceId, 25.5m, 60.0m), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}