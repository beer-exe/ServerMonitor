namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IMonitorHubService
    {
        Task SendDeviceUpdateAsync(Guid deviceId, decimal temperature, decimal humidity);
    }
}
