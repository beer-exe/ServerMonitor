using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApp.Application.Features.Users.Commands.CreateUser;
using ServerMonitorApp.Application.Features.Users.Commands.DeleteUser;
using ServerMonitorApp.Application.Features.Users.Commands.UpdateUser;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Features.Users.Queries.GetUserById;
using ServerMonitorApp.Application.Features.Users.Queries.GetUsers;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            Response<IEnumerable<UserDto>>? response = await _mediator.Send(new GetUsersQuery());
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            Response<UserDto>? response = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommand command)
        {
            Response<Guid>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            if (id != command.Id)
            {
                command.Id = id;
            }

            Response<Guid>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            Response<Guid>? response = await _mediator.Send(new DeleteUserCommand(id));
            return Ok(response);
        }
    }
}
