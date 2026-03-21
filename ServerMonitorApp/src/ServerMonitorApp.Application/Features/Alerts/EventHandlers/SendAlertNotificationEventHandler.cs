using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Alerts.Events;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.EventHandlers
{
    public class SendAlertNotificationEventHandler : INotificationHandler<DeviceAlertTriggeredEvent>
    {
        private readonly IMonitorHubDispatcher _hubDispatcher;
        private readonly IEmailService _emailService;
        private readonly IApplicationDbContext _context;
        private readonly ILogger<SendAlertNotificationEventHandler> _logger;

        public SendAlertNotificationEventHandler(IMonitorHubDispatcher hubDispatcher,IEmailService emailService, IApplicationDbContext context, ILogger<SendAlertNotificationEventHandler> logger)
        {
            _hubDispatcher = hubDispatcher;
            _emailService = emailService;
            _context = context;
            _logger = logger;
        }

        public async Task Handle(DeviceAlertTriggeredEvent notification, CancellationToken cancellationToken)
        {
            Alert? alert = notification.Alert;
            string groupName = $"Room_{alert.RoomId}";

            try
            {
                AlertDto? alertDto = new AlertDto
                {
                    Id = alert.Id,
                    RoomId = alert.RoomId,
                    DeviceId = alert.DeviceId,
                    Message = alert.Message,
                    Severity = alert.Severity,
                    IsResolved = alert.IsResolved,
                    CreatedAt = alert.CreatedAt,
                    UpdatedAt = alert.UpdatedAt
                };

                await _hubDispatcher.SendAlertToGroupAsync(groupName, alertDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi SignalR Alert cho Room {RoomId}", alert.RoomId);
            }

            try
            {
                List<string>? usersToNotify = await _context.UserRoomAccesses
                    .Include(ura => ura.User)
                    .Where(ura => ura.RoomId == alert.RoomId && ura.ReceiveAlerts == true)
                    .Select(ura => ura.User.Email)
                    .ToListAsync(cancellationToken);

                foreach (string? email in usersToNotify)
                {
                    string subject = $"[KHẨN CẤP] Cảnh báo hệ thống - Mức độ: {alert.Severity}";
                    string message = $"<h2>Cảnh báo hệ thống Server</h2>" +
                                     $"<p><strong>Thiết bị ID:</strong> {alert.DeviceId}</p>" +
                                     $"<p><strong>Nội dung:</strong> {alert.Message}</p>" +
                                     $"<p><strong>Thời gian:</strong> {alert.CreatedAt}</p>" +
                                     $"<p>Vui lòng đăng nhập hệ thống để kiểm tra và xử lý kịp thời.</p>";

                    await _emailService.SendAsync(email, subject, message, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi Email cảnh báo cho Alert ID {AlertId}", alert.Id);
            }
        }
    }
}