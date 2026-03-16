namespace ServerMonitorApp.Application.Features.AccessControl.DTOs
{
    public class UserRoomAccessDto
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public string? UserName { get; set; }
        public string? RoomName { get; set; }
        public bool? ReceiveAlerts { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
