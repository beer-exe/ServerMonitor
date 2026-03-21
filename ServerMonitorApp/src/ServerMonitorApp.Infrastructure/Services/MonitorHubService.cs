using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Infrastructure.Services
{
    public class MonitorHubService : IMonitorHubService
    {
        private readonly IMonitorHubDispatcher _dispatcher;
        private readonly IApplicationDbContext _context;

        public MonitorHubService(IMonitorHubDispatcher dispatcher, IApplicationDbContext context)
        {
            _dispatcher = dispatcher;
            _context = context;
        }

        public async Task SendDeviceUpdateAsync(Guid deviceId, decimal temperature, decimal humidity)
        {
            Device? device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == deviceId);

            if (device != null && device.RoomId.HasValue)
            {
                string groupName = $"Room_{device.RoomId.Value}";

                DeviceUpdateDto? updateData = new DeviceUpdateDto
                {
                    DeviceId = deviceId,
                    Temperature = temperature,
                    Humidity = humidity,
                    Timestamp = DateTime.Now
                };

                await _dispatcher.SendDeviceUpdateToGroupAsync(groupName, updateData);
            }
        }
    }
}