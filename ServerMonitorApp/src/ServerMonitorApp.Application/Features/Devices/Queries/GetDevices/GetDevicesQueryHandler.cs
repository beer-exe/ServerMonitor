using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Queries.GetDevices
{
    public class GetDevicesQueryHandler : IRequestHandler<GetDevicesQuery, Response<IEnumerable<DeviceDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetDevicesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<DeviceDto>>> Handle(GetDevicesQuery request, CancellationToken cancellationToken)
        {
            List<DeviceDto>? devices = await _context.Devices
                .Include(d => d.Room)
                .AsNoTracking()
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DeviceDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    RoomId = d.RoomId,
                    RoomName = d.Room != null ? d.Room.Name : null,
                    IsActive = d.IsActive ?? false,
                    TemperatureWarningThreshold = d.WarningTemp,
                    TemperatureCriticalThreshold = d.CriticalTemp,
                    HumidityWarningThreshold = d.WarningHumidity,
                    HumidityCriticalThreshold = d.CriticalHumidity,
                    LastSeen = d.LastSeen,
                    CreatedAt = d.CreatedAt,
                    UpdatedAt = d.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<DeviceDto>>(devices, "Lấy danh sách thiết bị thành công.");
        }
    }
}