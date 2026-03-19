using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.Queries.GetHistoricalData;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Dashboard.Queries
{
    public class GetHistoricalDataQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetHistoricalDataQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_DeviceNotFound_ThrowsApiException()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                Role = "ADMIN",
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 10
            };

            GetHistoricalDataQueryHandler handler = new GetHistoricalDataQueryHandler(_dbContext);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Thiết bị không tồn tại.", exception.Message);
        }

        [Fact]
        public async Task Handle_UserWithoutAccess_ThrowsUnauthorizedAccessException()
        {
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Restricted Room" });
            _dbContext.Devices.Add(new Device { Id = deviceId, RoomId = roomId, Name = "Secure Sensor" });

            await _dbContext.SaveChangesAsync();

            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = deviceId,
                UserId = userId,
                Role = "USER",
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 10
            };

            GetHistoricalDataQueryHandler handler = new GetHistoricalDataQueryHandler(_dbContext);

            UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(query, CancellationToken.None));
            Assert.Equal("Bạn không có quyền xem dữ liệu của thiết bị này.", exception.Message);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsPagedDataFilteredByDate()
        {
            Guid deviceId = Guid.NewGuid();
            Guid roomId = Guid.NewGuid();
            Guid adminId = Guid.NewGuid();
            DateTime baseTime = DateTime.UtcNow;

            _dbContext.Rooms.Add(new Room { Id = roomId, Name = "Server Room" });
            _dbContext.Devices.Add(new Device { Id = deviceId, RoomId = roomId, Name = "Temp Sensor" });

            _dbContext.SensorDatas.Add(new SensorData { Id = 1, DeviceId = deviceId, Temperature = 20, Humidity = 50, Timestamp = baseTime.AddHours(-10) });
            _dbContext.SensorDatas.Add(new SensorData { Id = 2, DeviceId = deviceId, Temperature = 21, Humidity = 51, Timestamp = baseTime.AddHours(-3) });
            _dbContext.SensorDatas.Add(new SensorData { Id = 3, DeviceId = deviceId, Temperature = 22, Humidity = 52, Timestamp = baseTime.AddHours(-2) });
            _dbContext.SensorDatas.Add(new SensorData { Id = 4, DeviceId = deviceId, Temperature = 23, Humidity = 53, Timestamp = baseTime.AddHours(-1) });
            _dbContext.SensorDatas.Add(new SensorData { Id = 5, DeviceId = deviceId, Temperature = 24, Humidity = 54, Timestamp = baseTime.AddHours(2) });

            await _dbContext.SaveChangesAsync();

            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = deviceId,
                UserId = adminId,
                Role = "ADMIN",
                StartTime = baseTime.AddHours(-4),
                EndTime = baseTime,
                PageNumber = 1,
                PageSize = 2
            };

            GetHistoricalDataQueryHandler handler = new GetHistoricalDataQueryHandler(_dbContext);

            PagedResponse<IEnumerable<ChartDataPointDto>> response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal("Truy xuất lịch sử thành công.", response.Message);

            Assert.Equal(1, response.PageNumber);
            Assert.Equal(2, response.PageSize);
            Assert.Equal(3, response.TotalRecords);
            Assert.Equal(2, response.TotalPages);

            Assert.NotNull(response.Data);
            Assert.Equal(2, response.Data!.Count());
            Assert.Equal(21m, response.Data!.First().Temperature);
            Assert.Equal(22m, response.Data!.Last().Temperature);
        }
    }
}