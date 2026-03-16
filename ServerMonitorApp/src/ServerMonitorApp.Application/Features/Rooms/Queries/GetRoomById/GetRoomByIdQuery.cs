using MediatR;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQuery : IRequest<Response<RoomDto>>
    {
        public Guid Id { get; set; }

        public GetRoomByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}