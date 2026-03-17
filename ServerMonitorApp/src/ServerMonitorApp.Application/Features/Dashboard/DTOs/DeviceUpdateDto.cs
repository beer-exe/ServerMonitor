namespace ServerMonitorApp.Application.Features.Dashboard.DTOs
{
    public class DeviceUpdateDto
    {
        public Guid DeviceId { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
