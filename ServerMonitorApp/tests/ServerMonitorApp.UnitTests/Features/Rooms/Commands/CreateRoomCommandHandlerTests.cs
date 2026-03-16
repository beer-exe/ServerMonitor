using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Commands
{
    public class CreateRoomCommandHandlerTests
    {
        private readonly ApplicationDbContext _dbContext;

        public CreateRoomCommandHandlerTests()
        {
            DbContextOptions<ApplicationDbContext>? options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Handle_ValidData_CreatesRoomAndReturnsGuid()
        {
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = "Server Room A",
                Location = "Tầng 1"
            };
            CreateRoomCommandHandler? handler = new CreateRoomCommandHandler(_dbContext);

            Response<Guid>? response = await handler.Handle(command, CancellationToken.None);

            Assert.True(response.Succeeded);
            Assert.NotEqual(Guid.Empty, response.Data);
            Assert.Equal("Tạo phòng thành công.", response.Message);

            Room? roomInDb = await _dbContext.Rooms.FindAsync(response.Data);
            Assert.NotNull(roomInDb);
            Assert.Equal("Server Room A", roomInDb.Name);
            Assert.Equal("Tầng 1", roomInDb.Location);
        }

        [Fact]
        public async Task Handle_DuplicateNameAndLocation_ThrowsApiException()
        {
            _dbContext.Rooms.Add(new Room { Id = Guid.NewGuid(), Name = "Server Room A", Location = "Tầng 1" });
            await _dbContext.SaveChangesAsync();

            CreateRoomCommand? command = new CreateRoomCommand { Name = "Server Room A", Location = "Tầng 1" };
            CreateRoomCommandHandler? handler = new CreateRoomCommandHandler(_dbContext);

            ApiException? exception = await Assert.ThrowsAsync<ApiException>(() => handler.Handle(command, CancellationToken.None));
            Assert.Equal("Đã tồn tại phòng có tên 'Server Room A' tại vị trí 'Tầng 1'. Vui lòng chọn tên hoặc vị trí khác.", exception.Message);
        }
    }
}
