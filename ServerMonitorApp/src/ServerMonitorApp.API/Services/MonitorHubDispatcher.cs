using Microsoft.AspNetCore.SignalR;
using ServerMonitorApp.API.Hubs;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;

namespace ServerMonitorApp.API.Services
{
    public class MonitorHubDispatcher : IMonitorHubDispatcher
    {
        private readonly IHubContext<MonitorHub, IMonitorClient> _hubContext;

        public MonitorHubDispatcher(IHubContext<MonitorHub, IMonitorClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendDeviceUpdateToGroupAsync(string groupName, DeviceUpdateDto update)
        {
            await _hubContext.Clients.Group(groupName).ReceiveDeviceUpdate(update);
        }

        public async Task SendAlertToGroupAsync(string groupName, AlertDto alert)
        {
            await _hubContext.Clients.Group(groupName).ReceiveAlert(alert);
        }
    }
}