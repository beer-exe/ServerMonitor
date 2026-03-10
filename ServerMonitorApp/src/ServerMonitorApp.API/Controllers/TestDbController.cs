using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Domain.Models;
using ServerMonitorApp.Infrastructure.Persistence;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDbController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestDbController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("check-connection")]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                List<Room>? rooms = await _context.Rooms.ToListAsync();

                return Ok(new
                {
                    Status = "Success",
                    Message = "Kết nối Database PostgreSQL thành công!",
                    TotalRoomsFetched = rooms.Count,
                    Data = rooms
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Không thể kết nối đến Database!",
                    ErrorDetail = ex.Message,
                    InnerException = ex.InnerException?.Message
                });
            }
        }
    }
}