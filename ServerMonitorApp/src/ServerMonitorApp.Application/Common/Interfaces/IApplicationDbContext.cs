using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<Alert> Alerts { get; set; }

        DbSet<Device> Devices { get; set; }

        DbSet<Room> Rooms { get; set; }

        DbSet<SensorData> SensorDatas { get; set; }

        DbSet<User> Users { get; set; }

        DbSet<UserRoomAccess> UserRoomAccesses { get; set; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
