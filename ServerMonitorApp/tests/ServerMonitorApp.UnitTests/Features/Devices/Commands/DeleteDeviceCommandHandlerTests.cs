using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Devices.Commands.DeleteDevice;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Devices.Commands
{
    public class DeleteDeviceCommandHandlerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_GivenValidId_DeletesDeviceAndReturnsTrue()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid deviceId = Guid.NewGuid();

            Device? device = new Device
            {
                Id = deviceId,
                Name = "Cảm biến cửa 1",
                IsActive = true
            };
            context.Devices.Add(device);
            await context.SaveChangesAsync();

            DeleteDeviceCommandHandler? handler = new DeleteDeviceCommandHandler(context);
            DeleteDeviceCommand? command = new DeleteDeviceCommand(deviceId);

            Response<bool>? result = await handler.Handle(command, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.True(result.Data);
            Assert.Equal("Xóa thiết bị thành công.", result.Message);

            Device? deviceInDb = await context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
            Assert.Null(deviceInDb);
        }

        [Fact]
        public async Task Handle_GivenInvalidId_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            DeleteDeviceCommandHandler? handler = new DeleteDeviceCommandHandler(context);
            Guid invalidDeviceId = Guid.NewGuid();
            DeleteDeviceCommand? command = new DeleteDeviceCommand(invalidDeviceId);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));

            Assert.Equal("Thiết bị không tồn tại.", exception.Message);
        }
    }
}