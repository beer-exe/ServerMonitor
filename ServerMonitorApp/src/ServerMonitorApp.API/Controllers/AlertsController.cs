using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Features.Alerts.Queries.GetAlertById;
using ServerMonitorApp.Application.Features.Alerts.Queries.GetAlerts;
using ServerMonitorApp.Application.Wrappers;
using System.Security.Claims;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AlertsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts([FromQuery] GetAlertsQuery query)
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new Response<string>("Không thể xác thực người dùng."));
            }    

            query.UserId = userId;
            query.Role = role ?? "USER";

            PagedResponse<IEnumerable<AlertDto>>? response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlertById(long id)
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new Response<string>("Không thể xác thực người dùng."));
            }

            GetAlertByIdQuery? query = new GetAlertByIdQuery
            {
                Id = id,
                UserId = userId,
                Role = role ?? "USER"
            };

            Response<AlertDto>? response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpPut("{id}/resolve")]
        public async Task<IActionResult> ResolveAlert(long id, [FromBody] ResolveAlertCommand command)
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return Unauthorized(new Response<string>("Không thể xác thực người dùng."));
            }

            command.Id = id;
            command.UserId = userId;
            command.Role = role ?? "USER";

            Response<long>? response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}