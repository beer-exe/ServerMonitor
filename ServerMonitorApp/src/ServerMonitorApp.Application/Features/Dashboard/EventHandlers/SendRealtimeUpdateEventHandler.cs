using MediatR;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.IoT.Events;

namespace ServerMonitorApp.Application.Features.Dashboard.EventHandlers
{
    public class SendRealtimeUpdateEventHandler : INotificationHandler<SensorDataRecordedEvent>
    {
        private readonly IMonitorHubService _monitorHubService;
        private readonly ILogger<SendRealtimeUpdateEventHandler> _logger;

        public SendRealtimeUpdateEventHandler(IMonitorHubService monitorHubService, ILogger<SendRealtimeUpdateEventHandler> logger)
        {
            _monitorHubService = monitorHubService;
            _logger = logger;
        }

        public async Task Handle(SensorDataRecordedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                await _monitorHubService.SendDeviceUpdateAsync(notification.DeviceId, notification.Temperature, notification.Humidity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gửi dữ liệu Real-time qua SignalR cho DeviceId: {DeviceId}", notification.DeviceId);
            }
        }
    }
}