using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.Queries.GetDashboard;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Dashboard.Queries
{
    public class GetDashboardQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetDashboardQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_AdminRole_ReturnsAllRooms_And_CalculatesDeviceStatusCorrectly()
        {
            Guid adminId = Guid.NewGuid();
            Guid room1Id = Guid.NewGuid();
            Guid room2Id = Guid.NewGuid();
            Guid device1Id = Guid.NewGuid();
            Guid device2Id = Guid.NewGuid();
            Guid device3Id = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = room1Id, Name = "Room 1" });
            _dbContext.Rooms.Add(new Room { Id = room2Id, Name = "Room 2" });

            _dbContext.Devices.Add(new Device
            {
                Id = device1Id,
                RoomId = room1Id,
                Name = "Device 1 - Online",
                LastSeen = DateTime.UtcNow.AddMinutes(-2)
            });
            _dbContext.SensorDatas.Add(new SensorData { Id = 1, DeviceId = device1Id, Temperature = 20, Humidity = 50, Timestamp = DateTime.UtcNow.AddMinutes(-3) });
            _dbContext.SensorDatas.Add(new SensorData { Id = 2, DeviceId = device1Id, Temperature = 25, Humidity = 60, Timestamp = DateTime.UtcNow.AddMinutes(-1) });

            _dbContext.Devices.Add(new Device
            {
                Id = device2Id,
                RoomId = room2Id,
                Name = "Device 2 - Offline",
                LastSeen = DateTime.UtcNow.AddMinutes(-10)
            });

            _dbContext.Devices.Add(new Device
            {
                Id = device3Id,
                RoomId = room2Id,
                Name = "Device 3 - Never Seen",
                LastSeen = null
            });

            await _dbContext.SaveChangesAsync();

            GetDashboardQuery query = new GetDashboardQuery { UserId = adminId, Role = "ADMIN" };
            GetDashboardQueryHandler handler = new GetDashboardQueryHandler(_dbContext);

            Response<IEnumerable<DashboardRoomDto>> response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(2, response.Data!.Count());

            DashboardRoomDto room1Dto = response.Data!.First(r => r.Id == room1Id);
            DashboardDeviceDto device1Dto = room1Dto.Devices.First();
            Assert.False(device1Dto.IsOffline);
            Assert.Equal(25m, device1Dto.CurrentTemperature);
            Assert.Equal(60m, device1Dto.CurrentHumidity);

            DashboardRoomDto room2Dto = response.Data!.First(r => r.Id == room2Id);
            DashboardDeviceDto device2Dto = room2Dto.Devices.First(d => d.Id == device2Id);
            Assert.True(device2Dto.IsOffline);
            Assert.Null(device2Dto.CurrentTemperature);

            DashboardDeviceDto device3Dto = room2Dto.Devices.First(d => d.Id == device3Id);
            Assert.True(device3Dto.IsOffline);
        }

        [Fact]
        public async Task Handle_UserRole_ReturnsOnlyAssignedRooms()
        {
            Guid userId = Guid.NewGuid();
            Guid roomAssignedId = Guid.NewGuid();
            Guid roomUnassignedId = Guid.NewGuid();

            _dbContext.Users.Add(new User { Id = userId, Username = "testuser", Email = "test@test.com", PasswordHash = "hash", Role = "USER" });
            _dbContext.Rooms.Add(new Room { Id = roomAssignedId, Name = "Assigned Room" });
            _dbContext.Rooms.Add(new Room { Id = roomUnassignedId, Name = "Unassigned Room" });

            _dbContext.UserRoomAccesses.Add(new UserRoomAccess { UserId = userId, RoomId = roomAssignedId, ReceiveAlerts = true });

            await _dbContext.SaveChangesAsync();

            GetDashboardQuery query = new GetDashboardQuery { UserId = userId, Role = "USER" };
            GetDashboardQueryHandler handler = new GetDashboardQueryHandler(_dbContext);

            Response<IEnumerable<DashboardRoomDto>> response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Single(response.Data!);
            Assert.Equal(roomAssignedId, response.Data!.First().Id);
            Assert.Equal("Assigned Room", response.Data!.First().Name);
        }
    }
}