namespace ServerMonitorApp.Domain.Models
{
    public partial class SensorData
    {
        public long Id { get; set; }
        public Guid? DeviceId { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public DateTime Timestamp { get; set; }
        public virtual Device? Device { get; set; }
    }
}
