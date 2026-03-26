using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice
{
    public class CreateDeviceCommandHandler : IRequestHandler<CreateDeviceCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;

        public CreateDeviceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<string>> Handle(CreateDeviceCommand request, CancellationToken cancellationToken)
        {
            if (request.RoomId.HasValue && !await _context.Rooms.AnyAsync(r => r.Id == request.RoomId.Value, cancellationToken))
            {
                throw new ApiException("Phòng không tồn tại.");
            }

            if (await _context.Devices.AnyAsync(d => d.Name == request.Name && d.RoomId == request.RoomId, cancellationToken))
            {
                string roomMsg = request.RoomId.HasValue ? "phòng này" : "nhóm chưa gán phòng";
                throw new ApiException($"Thiết bị với tên '{request.Name}' đã tồn tại trong {roomMsg}.");
            }

            Device? device = new Device
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                RoomId = request.RoomId,
                IsActive = request.IsActive,
                WarningTemp = request.TemperatureWarningThreshold,
                CriticalTemp = request.TemperatureCriticalThreshold,
                WarningHumidity = request.HumidityWarningThreshold,
                CriticalHumidity = request.HumidityCriticalThreshold,
                CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>(device.Id.ToString(), "Tạo thiết bị thành công.");
        }
    }
}