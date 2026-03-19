using MediatR;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.IoT.Events
{
    public class SensorDataRecordedEvent : INotification
    {
        public Guid DeviceId { get; set; }
        public long SensorDataId { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }

        public SensorDataRecordedEvent(Guid deviceId, long sensorDataId, decimal temperature, decimal humidity)
        {
            DeviceId = deviceId;
            SensorDataId = sensorDataId;
            Temperature = temperature;
            Humidity = humidity;
        }
    }
}