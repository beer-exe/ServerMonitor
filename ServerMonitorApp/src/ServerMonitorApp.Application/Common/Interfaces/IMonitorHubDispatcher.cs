using ServerMonitorApp.Application.Features.Dashboard.DTOs;

namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IMonitorHubDispatcher
    {
        Task SendDeviceUpdateToGroupAsync(string groupName, DeviceUpdateDto update);
    }
}
