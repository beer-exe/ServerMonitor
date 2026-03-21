namespace ServerMonitorApp.Application.Features.Alerts.DTOs
{
    public class AlertDto
    {
        public long Id { get; set; }
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }
        public Guid? DeviceId { get; set; }
        public string? DeviceName { get; set; }
        public string Message { get; set; } = null!;
        public string? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}