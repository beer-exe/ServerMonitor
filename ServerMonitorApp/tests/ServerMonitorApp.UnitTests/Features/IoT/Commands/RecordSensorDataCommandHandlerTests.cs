using MediatR;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData;
using ServerMonitorApp.Application.Features.IoT.Events;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.IoT.Commands
{
    public class RecordSensorDataCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly Mock<IMediator> _mediatorMock;

        public RecordSensorDataCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            _mediatorMock = new Mock<IMediator>();
        }

        [Fact]
        public async Task Handle_ValidData_RecordsData_PublishesEvent_AndReturnsId()
        {
            Guid deviceId = Guid.NewGuid();
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Sensor 1",
                IsActive = true
            });
            await _dbContext.SaveChangesAsync();

            RecordSensorDataCommand command = new RecordSensorDataCommand
            {
                DeviceId = deviceId,
                Temperature = 25.5m,
                Humidity = 60.0m
            };

            RecordSensorDataCommandHandler handler = new RecordSensorDataCommandHandler(
                _dbContext,
                _mediatorMock.Object);

            Response<long> response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Dữ liệu đã được ghi nhận.", response.Message);
            Assert.True(response.Data > 0);

            SensorData? sensorDataInDb = await _dbContext.SensorDatas.FirstOrDefaultAsync(s => s.Id == response.Data);
            Assert.NotNull(sensorDataInDb);
            Assert.Equal(25.5m, sensorDataInDb.Temperature);
            Assert.Equal(60.0m, sensorDataInDb.Humidity);

            Device? updatedDevice = await _dbContext.Devices.FindAsync(deviceId);
            Assert.NotNull(updatedDevice?.LastSeen);

            _mediatorMock.Verify(m => m.Publish(
                It.Is<SensorDataRecordedEvent>(e =>
                    e.DeviceId == deviceId &&
                    e.Temperature == 25.5m &&
                    e.Humidity == 60.0m &&
                    e.SensorDataId == response.Data),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DeviceNotFound_ThrowsApiException()
        {
            RecordSensorDataCommand command = new RecordSensorDataCommand
            {
                DeviceId = Guid.NewGuid(),
                Temperature = 25.5m,
                Humidity = 60.0m
            };

            RecordSensorDataCommandHandler handler = new RecordSensorDataCommandHandler(
                _dbContext,
                _mediatorMock.Object);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Thiết bị không tồn tại hoặc mã thiết bị không hợp lệ.", exception.Message);

            _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DeviceInactive_ThrowsApiException()
        {
            Guid deviceId = Guid.NewGuid();
            _dbContext.Devices.Add(new Device
            {
                Id = deviceId,
                Name = "Sensor 2",
                IsActive = false
            });
            await _dbContext.SaveChangesAsync();

            RecordSensorDataCommand command = new RecordSensorDataCommand
            {
                DeviceId = deviceId,
                Temperature = 25.5m,
                Humidity = 60.0m
            };

            RecordSensorDataCommandHandler handler = new RecordSensorDataCommandHandler(
                _dbContext,
                _mediatorMock.Object);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Thiết bị đang bị vô hiệu hóa. Không thể nhận dữ liệu.", exception.Message);

            _mediatorMock.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}