using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.RevokeRoomAccess
{
    public class RevokeRoomAccessCommand : IRequest<Response<string>>
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }

        public RevokeRoomAccessCommand(Guid userId, Guid roomId)
        {
            UserId = userId;
            RoomId = roomId;
        }
    }
}