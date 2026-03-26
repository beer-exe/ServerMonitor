using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Devices.Commands.UpdateDevice
{
    public class UpdateDeviceCommandHandler : IRequestHandler<UpdateDeviceCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateDeviceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<bool>> Handle(UpdateDeviceCommand request, CancellationToken cancellationToken)
        {
            Device? device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (device == null)
            {
                throw new ApiException("Thiết bị không tồn tại.");
            }

            if (await _context.Devices.AnyAsync(d => d.Name == request.Name && d.RoomId == request.RoomId && d.Id != request.Id, cancellationToken))
            {
                string roomMsg = request.RoomId.HasValue ? "phòng này" : "nhóm chưa gán phòng";
                throw new ApiException($"Thiết bị với tên '{request.Name}' đã tồn tại trong {roomMsg}.");
            }

            device.Name = request.Name;
            device.RoomId = request.RoomId;
            device.IsActive = request.IsActive;
            device.WarningTemp = request.TemperatureWarningThreshold;
            device.CriticalTemp = request.TemperatureCriticalThreshold;
            device.WarningHumidity = request.HumidityWarningThreshold;
            device.CriticalHumidity = request.HumidityCriticalThreshold;
            device.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<bool>(true, "Cập nhật thiết bị thành công.");
        }
    }
}