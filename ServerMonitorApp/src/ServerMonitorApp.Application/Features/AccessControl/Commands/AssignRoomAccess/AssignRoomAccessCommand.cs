using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.AssignRoomAccess
{
    public class AssignRoomAccessCommand : IRequest<Response<string>>
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public bool ReceiveAlerts { get; set; } = true;
    }
}