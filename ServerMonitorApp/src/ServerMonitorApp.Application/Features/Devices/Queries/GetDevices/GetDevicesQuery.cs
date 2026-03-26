using MediatR;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Queries.GetDevices
{
    public class GetDevicesQuery : IRequest<Response<IEnumerable<DeviceDto>>>
    {
    }
}