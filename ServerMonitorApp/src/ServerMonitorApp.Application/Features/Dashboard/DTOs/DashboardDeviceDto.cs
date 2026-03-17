namespace ServerMonitorApp.Application.Features.Dashboard.DTOs
{
    public class DashboardDeviceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool? IsActive { get; set; }
        public decimal? WarningTemp { get; set; }
        public decimal? CriticalTemp { get; set; }
        public decimal? WarningHumidity { get; set; }
        public decimal? CriticalHumidity { get; set; }
        public DateTime? LastSeen { get; set; }
        public decimal? CurrentTemperature { get; set; }
        public decimal? CurrentHumidity { get; set; }
        public bool IsOffline { get; set; }
    }
}