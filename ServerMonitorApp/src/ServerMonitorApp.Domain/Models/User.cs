namespace ServerMonitorApp.Domain.Models
{
    public partial class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Role { get; set; } = null!;

        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<UserRoomAccess> UserRoomAccesses { get; set; } = new List<UserRoomAccess>();
    }
}