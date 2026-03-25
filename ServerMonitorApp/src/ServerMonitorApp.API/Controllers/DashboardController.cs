using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Features.Dashboard.Queries.GetDashboard;
using ServerMonitorApp.Application.Features.Dashboard.Queries.GetHistoricalData;
using ServerMonitorApp.Application.Wrappers;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new Response<string>("Không thể xác thực thông tin người dùng."));
            }

            GetDashboardQuery? query = new GetDashboardQuery
            {
                UserId = userId,
                Role = role ?? "USER"
            };

            Response<IEnumerable<DashboardRoomDto>>? response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("history/devices/{deviceId}")]
        public async Task<IActionResult> GetHistoricalData
        (
            Guid deviceId, 
            [FromQuery] DateTime startTime, 
            [FromQuery] DateTime endTime , 
            [FromQuery] int pageNumber = 1, 
            [FromQuery] int pageSize = 50
        )
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new Response<string>("Không thể xác thực thông tin người dùng."));
            }

            GetHistoricalDataQuery? query = new GetHistoricalDataQuery
            {
                DeviceId = deviceId,
                UserId = userId,
                Role = role ?? "USER",
                StartTime = DateTime.SpecifyKind(startTime, DateTimeKind.Unspecified),
                EndTime = DateTime.SpecifyKind(endTime, DateTimeKind.Unspecified),
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            Response<IEnumerable<ChartDataPointDto>>? response = await _mediator.Send(query);
            return Ok(response);
        }
    }
}