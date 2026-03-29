using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom;
using ServerMonitorApp.Application.Features.Rooms.Commands.DeleteRoom;
using ServerMonitorApp.Application.Features.Rooms.Commands.UpdateRoom;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Features.Rooms.Queries.GetRoomById;
using ServerMonitorApp.Application.Features.Rooms.Queries.GetRooms;
using ServerMonitorApp.Application.Wrappers;
using System.Security.Claims;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoomsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query)
        {
            string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = User.FindFirstValue(ClaimTypes.Role);

            if (Guid.TryParse(userIdString, out Guid userId))
            {
                query.UserId = userId;
                query.Role = role ?? "USER";
            }

            Response<IEnumerable<RoomDto>>? response = await _mediator.Send(query);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(Guid id)
        {
            Response<RoomDto>? response = await _mediator.Send(new GetRoomByIdQuery(id));
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomCommand command)
        {
            Response<Guid>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateRoom(Guid id, [FromBody] UpdateRoomCommand command)
        {
            if (id != command.Id)
            {
                command.Id = id;
            }

            Response<Guid>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteRoom(Guid id)
        {
            Response<Guid>? response = await _mediator.Send(new DeleteRoomCommand(id));
            return Ok(response);
        }
    }
}
