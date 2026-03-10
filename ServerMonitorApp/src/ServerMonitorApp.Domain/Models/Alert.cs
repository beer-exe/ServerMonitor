namespace ServerMonitorApp.Domain.Models
{
    public partial class Alert
    {
        public long Id { get; set; }

        public Guid? RoomId { get; set; }

        public Guid? DeviceId { get; set; }

        public long? SensorDataId { get; set; }

        public string Message { get; set; } = null!;

        public bool? IsResolved { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Severity { get; set; }

        public virtual Device? Device { get; set; }

        public virtual Room? Room { get; set; }
    }
}
