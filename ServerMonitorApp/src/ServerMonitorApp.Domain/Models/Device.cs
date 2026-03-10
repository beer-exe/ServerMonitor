namespace ServerMonitorApp.Domain.Models
{
    public partial class Device
    {
        public Guid Id { get; set; }

        public Guid? RoomId { get; set; }

        public string Name { get; set; } = null!;

        public bool? IsActive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public decimal? WarningTemp { get; set; }

        public decimal? CriticalTemp { get; set; }

        public DateTime? LastSeen { get; set; }

        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

        public virtual Room? Room { get; set; }

        public virtual ICollection<SensorData> SensorDatas { get; set; } = new List<SensorData>();
    }
}