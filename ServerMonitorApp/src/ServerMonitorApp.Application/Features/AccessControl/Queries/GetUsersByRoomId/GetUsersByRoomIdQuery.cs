using MediatR;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Queries.GetUsersByRoomId
{
    public class GetUsersByRoomIdQuery : IRequest<Response<IEnumerable<UserRoomAccessDto>>>
    {
        public Guid RoomId { get; set; }

        public GetUsersByRoomIdQuery(Guid roomId)
        {
            RoomId = roomId;
        }
    }
}