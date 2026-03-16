using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommand : IRequest<Response<Guid>>
    {
        public string Name { get; set; } = null!;
        public string? Location { get; set; }
    }
}
