using MediatR;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Devices.Commands.UpdateDevice
{
    public class UpdateDeviceCommand : IRequest<Response<bool>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? RoomId { get; set; }
        public bool IsActive { get; set; }
        public decimal? TemperatureWarningThreshold { get; set; }
        public decimal? TemperatureCriticalThreshold { get; set; }
        public decimal? HumidityWarningThreshold { get; set; }
        public decimal? HumidityCriticalThreshold { get; set; }
    }
}