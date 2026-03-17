using MediatR;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData
{
    public class RecordSensorDataCommand : IRequest<Response<long>>
    {
        [JsonIgnore]
        public Guid DeviceId { get; set; }

        public decimal Temperature { get; set; }

        public decimal Humidity { get; set; }
    }
}