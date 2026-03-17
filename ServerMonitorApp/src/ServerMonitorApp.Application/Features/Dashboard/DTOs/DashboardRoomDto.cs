namespace ServerMonitorApp.Application.Features.Dashboard.DTOs
{
    public class DashboardRoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Location { get; set; }
        public List<DashboardDeviceDto> Devices { get; set; } = new List<DashboardDeviceDto>();
    }
}