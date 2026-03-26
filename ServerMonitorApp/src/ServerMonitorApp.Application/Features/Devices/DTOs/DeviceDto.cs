namespace ServerMonitorApp.Application.Features.Devices.DTOs
{
    public class DeviceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }
        public bool IsActive { get; set; }
        public decimal? TemperatureWarningThreshold { get; set; }
        public decimal? TemperatureCriticalThreshold { get; set; }
        public decimal? HumidityWarningThreshold { get; set; }
        public decimal? HumidityCriticalThreshold { get; set; }

        public DateTime? LastSeen { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}