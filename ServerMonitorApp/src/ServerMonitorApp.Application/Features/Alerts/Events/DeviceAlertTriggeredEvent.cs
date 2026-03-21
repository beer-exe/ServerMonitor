using MediatR;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.Events
{
    public class DeviceAlertTriggeredEvent : INotification
    {
        public Alert Alert { get; }

        public DeviceAlertTriggeredEvent(Alert alert)
        {
            Alert = alert;
        }
    }
}