using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Queries.GetDeviceById
{
    public class GetDeviceByIdQueryHandler : IRequestHandler<GetDeviceByIdQuery, Response<DeviceDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDeviceByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<DeviceDto>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
        {
            DeviceDto? device = await _context.Devices
                .Include(d => d.Room)
                .AsNoTracking()
                .Where(d => d.Id == request.Id)
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
                .FirstOrDefaultAsync(cancellationToken);

            if (device == null)
            {
                throw new ApiException("Thiết bị không tồn tại.");
            }

            return new Response<DeviceDto>(device, "Lấy thông tin thiết bị thành công.");
        }
    }
}