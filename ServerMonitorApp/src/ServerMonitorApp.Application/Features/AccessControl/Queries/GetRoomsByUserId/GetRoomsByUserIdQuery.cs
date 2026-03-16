using MediatR;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Queries.GetRoomsByUserId
{
    public class GetRoomsByUserIdQuery : IRequest<Response<IEnumerable<UserRoomAccessDto>>>
    {
        public Guid UserId { get; set; }

        public GetRoomsByUserIdQuery(Guid userId)
        {
            UserId = userId;
        }
    }
}