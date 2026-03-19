using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.IoT.Events;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData
{
    public class RecordSensorDataCommandHandler : IRequestHandler<RecordSensorDataCommand, Response<long>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMediator _mediator;

        public RecordSensorDataCommandHandler(IApplicationDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<Response<long>> Handle(RecordSensorDataCommand request, CancellationToken cancellationToken)
        {
            Device? device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);

            if (device == null)
            {
                throw new ApiException("Thiết bị không tồn tại hoặc mã thiết bị không hợp lệ.");
            }

            if (device.IsActive == false)
            {
                throw new ApiException("Thiết bị đang bị vô hiệu hóa. Không thể nhận dữ liệu.");
            }

            device.LastSeen = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            SensorData sensorData = new SensorData
            {
                DeviceId = request.DeviceId,
                Temperature = request.Temperature,
                Humidity = request.Humidity,
                Timestamp = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.SensorDatas.Add(sensorData);

            // TODO: Kích hoạt sự kiện kiểm tra ngưỡng cảnh báo (Alerts) tại đây

            await _context.SaveChangesAsync(cancellationToken);

            await _mediator.Publish(new SensorDataRecordedEvent(request.DeviceId, sensorData.Id, request.Temperature, request.Humidity), cancellationToken);

            return new Response<long>(sensorData.Id, "Dữ liệu đã được ghi nhận.");
        }
    }
}