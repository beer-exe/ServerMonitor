using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Commands.DeleteDevice
{
    public class DeleteDeviceCommand : IRequest<Response<bool>>
    {
        public Guid Id { get; set; }
        public DeleteDeviceCommand(Guid id) => Id = id;
    }
}