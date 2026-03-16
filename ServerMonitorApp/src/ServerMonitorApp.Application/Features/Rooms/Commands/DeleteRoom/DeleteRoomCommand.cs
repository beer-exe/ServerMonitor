using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public DeleteRoomCommand(Guid id)
        {
            Id = id;
        }
    }
}
