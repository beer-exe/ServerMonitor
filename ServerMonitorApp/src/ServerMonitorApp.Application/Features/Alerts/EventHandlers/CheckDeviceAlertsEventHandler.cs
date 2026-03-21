using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.Events;
using ServerMonitorApp.Application.Features.IoT.Events;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.EventHandlers
{
    public class CheckDeviceAlertsEventHandler : INotificationHandler<SensorDataRecordedEvent>
    {
        private readonly IApplicationDbContext _context;
        private readonly ILogger<CheckDeviceAlertsEventHandler> _logger;
        private readonly IMediator _mediator;

        public CheckDeviceAlertsEventHandler(IApplicationDbContext context, ILogger<CheckDeviceAlertsEventHandler> logger, IMediator mediator)
        {
            _context = context;
            _logger = logger;
            _mediator = mediator;
        }

        public async Task Handle(SensorDataRecordedEvent notification, CancellationToken cancellationToken)
        {
            try
            {
                Device? device = await _context.Devices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == notification.DeviceId, cancellationToken);

                if (device == null || device.IsActive == false || device.RoomId == null)
                {
                    return;
                }    

                List<Alert>? alertsToCreate = new List<Alert>();

                bool hasUnresolvedTempAlert = await _context.Alerts
                    .AnyAsync(a => a.DeviceId == device.Id && (a.IsResolved == false || a.IsResolved == null) && a.Message.Contains("Nhiệt độ"), cancellationToken);

                bool hasUnresolvedHumAlert = await _context.Alerts
                    .AnyAsync(a => a.DeviceId == device.Id && (a.IsResolved == false || a.IsResolved == null) && a.Message.Contains("Độ ẩm"), cancellationToken);

                if (!hasUnresolvedTempAlert)
                {
                    if (device.CriticalTemp.HasValue && notification.Temperature >= device.CriticalTemp.Value)
                    {
                        alertsToCreate.Add(CreateAlertRecord(
                            device, 
                            notification.SensorDataId, 
                            $"Nhiệt độ VƯỢT NGƯỠNG NGUY HIỂM: {notification.Temperature}°C (Ngưỡng cài đặt: {device.CriticalTemp}°C)", 
                            "CRITICAL"
                        ));
                    }
                    else if (device.WarningTemp.HasValue && notification.Temperature >= device.WarningTemp.Value)
                    {
                        alertsToCreate.Add(CreateAlertRecord(
                            device, 
                            notification.SensorDataId, 
                            $"Nhiệt độ cảnh báo cao: {notification.Temperature}°C (Ngưỡng cài đặt: {device.WarningTemp}°C)", 
                            "WARNING"
                        ));
                    }
                }

                if (!hasUnresolvedHumAlert)
                {
                    if (device.CriticalHumidity.HasValue && notification.Humidity >= device.CriticalHumidity.Value)
                    {
                        alertsToCreate.Add(CreateAlertRecord(
                            device, 
                            notification.SensorDataId, 
                            $"Độ ẩm VƯỢT NGƯỠNG NGUY HIỂM: {notification.Humidity}% (Ngưỡng cài đặt: {device.CriticalHumidity}%)", 
                            "CRITICAL"
                        ));
                    }
                    else if (device.WarningHumidity.HasValue && notification.Humidity >= device.WarningHumidity.Value)
                    {
                        alertsToCreate.Add(CreateAlertRecord(
                            device, 
                            notification.SensorDataId, 
                            $"Độ ẩm cảnh báo cao: {notification.Humidity}% (Ngưỡng cài đặt: {device.WarningHumidity}%)", 
                            "WARNING"
                        ));
                    }
                }

                if (alertsToCreate.Any())
                {
                    _context.Alerts.AddRange(alertsToCreate);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogWarning("Đã tự động tạo {Count} cảnh báo mới cho thiết bị {DeviceName} (ID: {DeviceId})", alertsToCreate.Count, device.Name, device.Id);

                    foreach (Alert? alert in alertsToCreate)
                    {
                        await _mediator.Publish(new DeviceAlertTriggeredEvent(alert), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Xảy ra lỗi trong hệ thống khi kiểm tra ngưỡng cảnh báo cho DeviceId: {DeviceId}", notification.DeviceId);
            }
        }

        private Alert CreateAlertRecord(Device device, long sensorDataId, string message, string severity)
        {
            return new Alert
            {
                RoomId = device.RoomId,
                DeviceId = device.Id,
                SensorDataId = sensorDataId,
                Message = message,
                Severity = severity,
                IsResolved = false,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
        }
    }
}