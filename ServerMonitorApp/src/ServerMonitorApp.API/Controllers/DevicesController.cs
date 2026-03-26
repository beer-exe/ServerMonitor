using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice;
using ServerMonitorApp.Application.Features.Devices.Commands.DeleteDevice;
using ServerMonitorApp.Application.Features.Devices.Commands.UpdateDevice;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Features.Devices.Queries.GetDeviceById;
using ServerMonitorApp.Application.Features.Devices.Queries.GetDevices;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DevicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            Response<IEnumerable<DeviceDto>>? response = await _mediator.Send(new GetDevicesQuery());
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceById(Guid id)
        {
            Response<DeviceDto>? response = await _mediator.Send(new GetDeviceByIdQuery(id));
            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceCommand command)
        {
            Response<string>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] UpdateDeviceCommand command)
        {
            command.Id = id;
            Response<bool>? response = await _mediator.Send(command);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            Response<bool>? response = await _mediator.Send(new DeleteDeviceCommand(id));
            return Ok(response);
        }
    }
}