namespace ServerMonitorApp.Application.Features.Rooms.DTOs
{
    public class RoomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Location { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
