using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData
{
    public class RecordSensorDataCommandHandler : IRequestHandler<RecordSensorDataCommand, Response<long>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMonitorHubService _monitorHubService;
        private readonly ILogger<RecordSensorDataCommandHandler> _logger;

        public RecordSensorDataCommandHandler(IApplicationDbContext context, IMonitorHubService monitorHubService, ILogger<RecordSensorDataCommandHandler> logger)
        {
            _context = context;
            _monitorHubService = monitorHubService;
            _logger = logger;
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

            try
            {
                await _monitorHubService.SendDeviceUpdateAsync(request.DeviceId, request.Temperature, request.Humidity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lỗi khi gửi dữ liệu Real-time qua SignalR cho DeviceId: {DeviceId}", request.DeviceId);
            }

            return new Response<long>(sensorData.Id, "Dữ liệu đã được ghi nhận.");
        }
    }
}