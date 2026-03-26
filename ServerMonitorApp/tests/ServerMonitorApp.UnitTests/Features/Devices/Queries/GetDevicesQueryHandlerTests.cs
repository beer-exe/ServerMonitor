using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Features.Devices.Queries.GetDevices;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Devices.Queries
{
    public class GetDevicesQueryHandlerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_WhenCalled_ReturnsAllDevicesOrderedByCreatedAtDescending()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid roomId1 = Guid.NewGuid();
            Guid roomId2 = Guid.NewGuid();

            context.Rooms.AddRange(
                new Room { Id = roomId1, Name = "Phòng 1" },
                new Room { Id = roomId2, Name = "Phòng 2" }
            );

            context.Devices.AddRange(
                new Device { Id = Guid.NewGuid(), Name = "Device 1", RoomId = roomId1, CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
                new Device { Id = Guid.NewGuid(), Name = "Device 2", RoomId = roomId2, CreatedAt = DateTime.UtcNow.AddMinutes(-5) },
                new Device { Id = Guid.NewGuid(), Name = "Device 3", RoomId = null, CreatedAt = DateTime.UtcNow }
            );

            await context.SaveChangesAsync();

            GetDevicesQueryHandler? handler = new GetDevicesQueryHandler(context);
            GetDevicesQuery? query = new GetDevicesQuery();

            Response<IEnumerable<DeviceDto>>? result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);

            List<DeviceDto>? devicesList = result.Data.ToList();
            Assert.Equal(3, devicesList.Count);

            Assert.Equal("Device 3", devicesList[0].Name);
            Assert.Null(devicesList[0].RoomName);

            Assert.Equal("Device 2", devicesList[1].Name);
            Assert.Equal("Phòng 2", devicesList[1].RoomName);

            Assert.Equal("Device 1", devicesList[2].Name);
            Assert.Equal("Phòng 1", devicesList[2].RoomName);
        }

        [Fact]
        public async Task Handle_WhenNoDevicesExist_ReturnsEmptyList()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            GetDevicesQueryHandler? handler = new GetDevicesQueryHandler(context);
            GetDevicesQuery? query = new GetDevicesQuery();

            Response<IEnumerable<DeviceDto>>? result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
    }
}