namespace ServerMonitorApp.Domain.Models
{

    public partial class UserRoomAccess
    {
        public Guid UserId { get; set; }

        public Guid RoomId { get; set; }

        public bool? ReceiveAlerts { get; set; }

        public virtual Room Room { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }
}
