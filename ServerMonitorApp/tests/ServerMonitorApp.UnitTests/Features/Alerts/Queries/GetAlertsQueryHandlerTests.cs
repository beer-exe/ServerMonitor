using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Alerts.Queries.GetAlerts;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Alerts.Queries
{
    public class GetAlertsQueryHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public GetAlertsQueryHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        private async Task SeedDataAsync(Guid adminId, Guid userId, Guid room1Id, Guid room2Id)
        {
            _dbContext.Users.Add(new User { Id = adminId, Username = "admin", Email = "admin@test.com", PasswordHash = "hash", Role = "ADMIN" });
            _dbContext.Users.Add(new User { Id = userId, Username = "user", Email = "user@test.com", PasswordHash = "hash", Role = "USER" });

            _dbContext.Rooms.Add(new Room { Id = room1Id, Name = "Room 1" });
            _dbContext.Rooms.Add(new Room { Id = room2Id, Name = "Room 2" });

            _dbContext.UserRoomAccesses.Add(new UserRoomAccess { UserId = userId, RoomId = room1Id, ReceiveAlerts = true });

            _dbContext.Alerts.AddRange(new List<Alert>
            {
                new Alert { Id = 1, RoomId = room1Id, Severity = "CRITICAL", IsResolved = false, CreatedAt = DateTime.UtcNow, Message = "Lỗi 1" },
                new Alert { Id = 2, RoomId = room1Id, Severity = "WARNING", IsResolved = true, CreatedAt = DateTime.UtcNow.AddMinutes(-1), Message = "Lỗi 2" },
                new Alert { Id = 3, RoomId = room1Id, Severity = "CRITICAL", IsResolved = false, CreatedAt = DateTime.UtcNow.AddMinutes(-2), Message = "Lỗi 3" },
                new Alert { Id = 4, RoomId = room2Id, Severity = "OFFLINE", IsResolved = false, CreatedAt = DateTime.UtcNow.AddMinutes(-3), Message = "Lỗi 4" },
                new Alert { Id = 5, RoomId = room2Id, Severity = "WARNING", IsResolved = true, CreatedAt = DateTime.UtcNow.AddMinutes(-4), Message = "Lỗi 5" }
            });

            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task Handle_AdminRole_ReturnsAllAlerts()
        {
            Guid adminId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid room1Id = Guid.NewGuid();
            Guid room2Id = Guid.NewGuid();
            await SeedDataAsync(adminId, userId, room1Id, room2Id);

            GetAlertsQuery? query = new GetAlertsQuery
            {
                UserId = adminId,
                Role = "ADMIN",
                PageNumber = 1,
                PageSize = 10
            };
            GetAlertsQueryHandler? handler = new GetAlertsQueryHandler(_dbContext);

            PagedResponse<IEnumerable<AlertDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(5, response.TotalRecords);
            Assert.Equal(5, response.Data!.Count());
        }

        [Fact]
        public async Task Handle_UserRole_ReturnsOnlyAllowedAlerts()
        {
            Guid adminId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid room1Id = Guid.NewGuid();
            Guid room2Id = Guid.NewGuid();
            await SeedDataAsync(adminId, userId, room1Id, room2Id);

            GetAlertsQuery? query = new GetAlertsQuery
            {
                UserId = userId,
                Role = "USER",
                PageNumber = 1,
                PageSize = 10
            };
            GetAlertsQueryHandler? handler = new GetAlertsQueryHandler(_dbContext);

            PagedResponse<IEnumerable<AlertDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(3, response.TotalRecords);
            Assert.All(response.Data!, a => Assert.Equal(room1Id, a.RoomId));
        }

        [Fact]
        public async Task Handle_FilterParameters_FiltersDataCorrectly()
        {
            Guid adminId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid room1Id = Guid.NewGuid();
            Guid room2Id = Guid.NewGuid();
            await SeedDataAsync(adminId, userId, room1Id, room2Id);

            GetAlertsQuery? query = new GetAlertsQuery
            {
                UserId = adminId,
                Role = "ADMIN",
                RoomId = room1Id,
                Severity = "CRITICAL",
                IsResolved = false,
                PageNumber = 1,
                PageSize = 10
            };
            GetAlertsQueryHandler? handler = new GetAlertsQueryHandler(_dbContext);

            PagedResponse<IEnumerable<AlertDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(2, response.TotalRecords);
            Assert.All(response.Data!, a =>
            {
                Assert.Equal(room1Id, a.RoomId);
                Assert.Equal("CRITICAL", a.Severity);
                Assert.False(a.IsResolved);
            });
        }

        [Fact]
        public async Task Handle_Pagination_CalculatesCorrectly()
        {
            Guid adminId = Guid.NewGuid();
            Guid userId = Guid.NewGuid();
            Guid room1Id = Guid.NewGuid();
            Guid room2Id = Guid.NewGuid();
            await SeedDataAsync(adminId, userId, room1Id, room2Id);

            GetAlertsQuery? query = new GetAlertsQuery
            {
                UserId = adminId,
                Role = "ADMIN",
                PageNumber = 2,
                PageSize = 2
            };
            GetAlertsQueryHandler? handler = new GetAlertsQueryHandler(_dbContext);

            PagedResponse<IEnumerable<AlertDto>>? response = await handler.Handle(query, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.Equal(5, response.TotalRecords);
            Assert.Equal(3, response.TotalPages);
            Assert.Equal(2, response.Data!.Count());

            Assert.Contains(response.Data!, a => a.Id == 3);
            Assert.Contains(response.Data!, a => a.Id == 4);
        }
    }
}