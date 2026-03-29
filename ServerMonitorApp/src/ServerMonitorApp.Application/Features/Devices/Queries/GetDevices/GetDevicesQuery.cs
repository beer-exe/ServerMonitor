using MediatR;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Devices.Queries.GetDevices
{
    public class GetDevicesQuery : IRequest<Response<IEnumerable<DeviceDto>>>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public string? Role { get; set; } = null!;
    }
}