using ServerMonitorApp.Application.Features.Dashboard.DTOs;

namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IMonitorClient
    {
        Task ReceiveDeviceUpdate(DeviceUpdateDto update);
    }
}