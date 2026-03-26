using MediatR;
using ServerMonitorApp.Application.Features.Devices.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Queries.GetDeviceById
{
    public class GetDeviceByIdQuery : IRequest<Response<DeviceDto>>
    {
        public Guid Id { get; set; }
        public GetDeviceByIdQuery(Guid id) => Id = id;
    }
}