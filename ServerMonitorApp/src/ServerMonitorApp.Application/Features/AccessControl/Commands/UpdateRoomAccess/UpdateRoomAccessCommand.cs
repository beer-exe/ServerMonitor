using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess
{
    public class UpdateRoomAccessCommand : IRequest<Response<string>>
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public bool ReceiveAlerts { get; set; }
    }
}