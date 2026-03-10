namespace ServerMonitorApp.Domain.Models
{
    public partial class Room
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Location { get; set; }

        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Alert> Alerts { get; set; } = new List<Alert>();

        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();

        public virtual ICollection<UserRoomAccess> UserRoomAccesses { get; set; } = new List<UserRoomAccess>();
    }
}
