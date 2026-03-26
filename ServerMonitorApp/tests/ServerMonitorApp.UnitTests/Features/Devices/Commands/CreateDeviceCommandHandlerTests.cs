using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Devices.Commands
{
    public class CreateDeviceCommandHandlerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesDeviceAndReturnsId()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid roomId = Guid.NewGuid();
            context.Rooms.Add(new Room { Id = roomId, Name = "Phòng Server 1" });
            await context.SaveChangesAsync();

            CreateDeviceCommandHandler? handler = new CreateDeviceCommandHandler(context);
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Cảm biến nhiệt độ 1",
                RoomId = roomId,
                IsActive = true,
                TemperatureWarningThreshold = 25,
                TemperatureCriticalThreshold = 35
            };

            Response<string>? result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);

            Device? deviceInDb = await context.Devices.FirstOrDefaultAsync(d => d.Name == "Cảm biến nhiệt độ 1");
            Assert.NotNull(deviceInDb);
            Assert.Equal(roomId, deviceInDb.RoomId);
            Assert.Equal(25, deviceInDb.WarningTemp);
        }

        [Fact]
        public async Task Handle_RoomDoesNotExist_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            CreateDeviceCommandHandler? handler = new CreateDeviceCommandHandler(context);
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Thiết bị mới",
                RoomId = Guid.NewGuid()
            };

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Phòng không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_DeviceNameAlreadyExistsInSameRoom_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid roomId = Guid.NewGuid();

            context.Rooms.Add(new Room { Id = roomId, Name = "Phòng Mạng" });
            context.Devices.Add(new Device { Id = Guid.NewGuid(), Name = "Cảm biến A", RoomId = roomId });
            await context.SaveChangesAsync();

            CreateDeviceCommandHandler? handler = new CreateDeviceCommandHandler(context);
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Cảm biến A",
                RoomId = roomId
            };

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Thiết bị với tên 'Cảm biến A' đã tồn tại trong phòng này.", exception.Message);
        }

        [Fact]
        public async Task Handle_DeviceNameAlreadyExistsWithNoRoom_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            context.Devices.Add(new Device { Id = Guid.NewGuid(), Name = "Cảm biến B", RoomId = null });
            await context.SaveChangesAsync();

            CreateDeviceCommandHandler? handler = new CreateDeviceCommandHandler(context);
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Cảm biến B",
                RoomId = null
            };

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Thiết bị với tên 'Cảm biến B' đã tồn tại trong nhóm chưa gán phòng.", exception.Message);
        }
    }
}