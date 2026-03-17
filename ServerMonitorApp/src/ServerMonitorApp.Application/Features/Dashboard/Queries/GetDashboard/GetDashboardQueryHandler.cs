using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, Response<IEnumerable<DashboardRoomDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetDashboardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<DashboardRoomDto>>> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Room> query = _context.Rooms.AsNoTracking();

            if (request.Role != "ADMIN")
            {
                query = query.Where(r => r.UserRoomAccesses.Any(ura => ura.UserId == request.UserId));
            }

            List<DashboardRoomDto>? rooms = await query
                .Select(r => new DashboardRoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Location = r.Location,
                    Devices = r.Devices.Select(d => new DashboardDeviceDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        IsActive = d.IsActive,
                        WarningTemp = d.WarningTemp,
                        CriticalTemp = d.CriticalTemp,
                        WarningHumidity = d.WarningHumidity,
                        CriticalHumidity = d.CriticalHumidity,
                        LastSeen = d.LastSeen,
                        CurrentTemperature = d.SensorDatas.OrderByDescending(s => s.Timestamp).Select(s => (decimal?)s.Temperature).FirstOrDefault(),
                        CurrentHumidity = d.SensorDatas.OrderByDescending(s => s.Timestamp).Select(s => (decimal?)s.Humidity).FirstOrDefault()
                    }).ToList()
                }).ToListAsync(cancellationToken);

            DateTime now = DateTime.UtcNow;
            foreach (DashboardRoomDto? room in rooms)
            {
                foreach (DashboardDeviceDto? device in room.Devices)
                {
                    device.IsOffline = !device.LastSeen.HasValue || (now - device.LastSeen.Value).TotalMinutes > 5;
                }
            }

            return new Response<IEnumerable<DashboardRoomDto>>(rooms, "Lấy dữ liệu Dashboard thành công.");
        }
    }
}