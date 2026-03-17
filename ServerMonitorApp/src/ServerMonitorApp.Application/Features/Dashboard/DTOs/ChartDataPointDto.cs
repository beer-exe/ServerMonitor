namespace ServerMonitorApp.Application.Features.Dashboard.DTOs
{
    public class ChartDataPointDto
    {
        public DateTime Timestamp { get; set; }
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
    }
}