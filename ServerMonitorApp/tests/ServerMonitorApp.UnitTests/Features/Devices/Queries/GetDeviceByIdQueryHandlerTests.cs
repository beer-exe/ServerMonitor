using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Features.Devices.Queries.GetDeviceById;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Devices.Queries
{
    public class GetDeviceByIdQueryHandlerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_GivenValidId_ReturnsDeviceDto()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();

            Room? room = new Room { Id = roomId, Name = "Phòng Server 1" };
            Device? device = new Device
            {
                Id = deviceId,
                Name = "Cảm biến nhiệt độ 1",
                RoomId = roomId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            context.Rooms.Add(room);
            context.Devices.Add(device);
            await context.SaveChangesAsync();

            GetDeviceByIdQueryHandler? handler = new GetDeviceByIdQueryHandler(context);
            GetDeviceByIdQuery? query = new GetDeviceByIdQuery(deviceId);

            Response<DeviceDto>? result = await handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Data);
            Assert.Equal(deviceId, result.Data.Id);
            Assert.Equal("Cảm biến nhiệt độ 1", result.Data.Name);
            Assert.Equal("Phòng Server 1", result.Data.RoomName);
        }

        [Fact]
        public async Task Handle_GivenInvalidId_ThrowsApiException()
        {
            ApplicationDbContext? context = GetInMemoryDbContext();
            GetDeviceByIdQueryHandler? handler = new GetDeviceByIdQueryHandler(context);
            Guid invalidDeviceId = Guid.NewGuid();
            GetDeviceByIdQuery? query = new GetDeviceByIdQuery(invalidDeviceId);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Thiết bị không tồn tại.", exception.Message);
        }
    }
}