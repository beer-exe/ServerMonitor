using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;

namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IMonitorHubDispatcher
    {
        Task SendDeviceUpdateToGroupAsync(string groupName, DeviceUpdateDto update);
        Task SendAlertToGroupAsync(string groupName, AlertDto alert);
    }
}
