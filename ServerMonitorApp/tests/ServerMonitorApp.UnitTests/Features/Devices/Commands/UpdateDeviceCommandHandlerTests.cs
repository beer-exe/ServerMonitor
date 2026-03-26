using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Devices.Commands.UpdateDevice;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Devices.Commands
{
    public class UpdateDeviceCommandHandlerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidCommand_UpdatesDeviceAndReturnsTrue()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            context.Rooms.Add(new Room { Id = roomId, Name = "Phòng Mạng" });
            context.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Cảm biến cũ",
                RoomId = roomId,
                IsActive = false
            });
            await context.SaveChangesAsync();

            UpdateDeviceCommandHandler? handler = new UpdateDeviceCommandHandler(context);
            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = deviceId,
                Name = "Cảm biến mới",
                RoomId = roomId,
                IsActive = true,
                TemperatureWarningThreshold = 25,
                TemperatureCriticalThreshold = 35
            };

            Response<bool>? result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Cập nhật thiết bị thành công.", result.Message);

            Device? updatedDevice = await context.Devices.FirstAsync(d => d.Id == deviceId);
            Assert.Equal("Cảm biến mới", updatedDevice.Name);
            Assert.True(updatedDevice.IsActive);
            Assert.Equal(25, updatedDevice.WarningTemp);
            Assert.Equal(35, updatedDevice.CriticalTemp);
        }

        [Fact]
        public async Task Handle_DeviceDoesNotExist_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            UpdateDeviceCommandHandler? handler = new UpdateDeviceCommandHandler(context);
            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = Guid.NewGuid(),
                Name = "Cảm biến mới"
            };

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Thiết bị không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_DeviceNameAlreadyExistsInSameRoom_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid roomId = Guid.NewGuid();
            Guid device1Id = Guid.NewGuid();
            Guid device2Id = Guid.NewGuid();

            context.Rooms.Add(new Room { Id = roomId, Name = "Phòng Server 1" });
            context.Devices.Add(new Device { Id = device1Id, Name = "Cảm biến A", RoomId = roomId });
            context.Devices.Add(new Device { Id = device2Id, Name = "Cảm biến B", RoomId = roomId });
            await context.SaveChangesAsync();

            UpdateDeviceCommandHandler? handler = new UpdateDeviceCommandHandler(context);

            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = device2Id,
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
            Guid device1Id = Guid.NewGuid();
            Guid device2Id = Guid.NewGuid();

            context.Devices.Add(new Device { Id = device1Id, Name = "Sensor Trống", RoomId = null });
            context.Devices.Add(new Device { Id = device2Id, Name = "Sensor Cũ", RoomId = null });
            await context.SaveChangesAsync();

            UpdateDeviceCommandHandler? handler = new UpdateDeviceCommandHandler(context);
            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = device2Id,
                Name = "Sensor Trống",
                RoomId = null
            };

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Thiết bị với tên 'Sensor Trống' đã tồn tại trong nhóm chưa gán phòng.", exception.Message);
        }
    }
}