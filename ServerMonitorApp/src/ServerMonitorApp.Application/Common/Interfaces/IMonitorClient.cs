using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;

namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IMonitorClient
    {
        Task ReceiveDeviceUpdate(DeviceUpdateDto update);
        Task ReceiveAlert(AlertDto alert);
    }
}