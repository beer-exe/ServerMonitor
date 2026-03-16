using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApp.Application.Features.AccessControl.Commands.AssignRoomAccess;
using ServerMonitorApp.Application.Features.AccessControl.Commands.RevokeRoomAccess;
using ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Features.AccessControl.Queries.GetRoomsByUserId;
using ServerMonitorApp.Application.Features.AccessControl.Queries.GetUsersByRoomId;
using ServerMonitorApp.Application.Wrappers;
using System.Security.Claims;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/access")]
    [Authorize]
    public class UserRoomAccessController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserRoomAccessController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("users/{userId}/rooms")]
        public async Task<IActionResult> GetRoomsByUser(Guid userId)
        {
            string? currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool isAdmin = User.IsInRole("ADMIN");

            if (!isAdmin && currentUserId != userId.ToString())
            {
                return Forbid();
            }

            Response<IEnumerable<UserRoomAccessDto>>? response = await _mediator.Send(new GetRoomsByUserIdQuery(userId));
            return Ok(response);
        }

        [HttpGet("rooms/{roomId}/users")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetUsersByRoom(Guid roomId)
        {
            Response<IEnumerable<UserRoomAccessDto>>? response = await _mediator.Send(new GetUsersByRoomIdQuery(roomId));
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> AssignAccess([FromBody] AssignRoomAccessCommand command)
        {
            Response<string>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateAccess([FromBody] UpdateRoomAccessCommand command)
        {
            Response<string>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("users/{userId}/rooms/{roomId}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RevokeAccess(Guid userId, Guid roomId)
        {
            Response<string>? response = await _mediator.Send(new RevokeRoomAccessCommand(userId, roomId));
            return Ok(response);
        }
    }
}