using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IotController : ControllerBase
    {
        private readonly IMediator _mediator;

        public IotController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("data")]
        public async Task<IActionResult> RecordData([FromBody] RecordSensorDataCommand command)
        {
            if (!Request.Headers.TryGetValue("X-Device-Id", out StringValues deviceIdHeader) ||
                !Guid.TryParse(deviceIdHeader, out Guid deviceId))
            {
                return BadRequest(new Response<string>("Thiếu hoặc sai định dạng X-Device-Id trong Header. Yêu cầu định dạng Guid."));
            }

            command.DeviceId = deviceId;

            Response<long>? response = await _mediator.Send(command);
            return Ok(response);
        }
    }
}