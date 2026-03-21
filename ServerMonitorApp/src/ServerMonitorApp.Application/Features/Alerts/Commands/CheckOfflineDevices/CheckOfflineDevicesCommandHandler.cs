using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.Events;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.Commands.CheckOfflineDevices
{
    public class CheckOfflineDevicesCommandHandler : IRequestHandler<CheckOfflineDevicesCommand, int>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger<CheckOfflineDevicesCommandHandler> _logger;

        public CheckOfflineDevicesCommandHandler(IApplicationDbContext context, IMediator mediator, ILogger<CheckOfflineDevicesCommandHandler> logger)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<int> Handle(CheckOfflineDevicesCommand request, CancellationToken cancellationToken)
        {
            int alertsCreatedCount = 0;
            DateTime threshold = DateTime.SpecifyKind(DateTime.UtcNow.AddMinutes(-5), DateTimeKind.Unspecified);

            List<Device>? offlineDevices = await _context.Devices
                .Where(d => d.IsActive == true && (d.LastSeen == null || d.LastSeen < threshold))
                .ToListAsync(cancellationToken);

            foreach (Device? device in offlineDevices)
            {
                bool hasUnresolvedOfflineAlert = await _context.Alerts
                    .AnyAsync(a => a.DeviceId == device.Id && a.IsResolved == false && a.Severity == "OFFLINE", cancellationToken);

                if (!hasUnresolvedOfflineAlert)
                {
                    Alert? alert = new Alert
                    {
                        RoomId = device.RoomId,
                        DeviceId = device.Id,
                        Message = $"Thiết bị mất kết nối (Offline). Lần cuối nhìn thấy: {device.LastSeen?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Chưa từng kết nối"}.",
                        Severity = "OFFLINE",
                        IsResolved = false,
                        CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                        UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
                    };

                    _context.Alerts.Add(alert);
                    alertsCreatedCount++;

                    await _mediator.Publish(new DeviceAlertTriggeredEvent(alert), cancellationToken);

                    _logger.LogWarning("Đã phát hiện và tạo cảnh báo OFFLINE cho thiết bị {DeviceId}", device.Id);
                }
            }

            if (alertsCreatedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return alertsCreatedCount;
        }
    }
}