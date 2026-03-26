using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice
{
    public class CreateDeviceCommand : IRequest<Response<string>>
    {
        public string Name { get; set; } = null!;
        public Guid? RoomId { get; set; }
        public bool IsActive { get; set; }
        public decimal? TemperatureWarningThreshold { get; set; }
        public decimal? TemperatureCriticalThreshold { get; set; }
        public decimal? HumidityWarningThreshold { get; set; }
        public decimal? HumidityCriticalThreshold { get; set; }
    }
}