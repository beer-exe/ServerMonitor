using MediatR;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Rooms.Queries.GetRooms
{
    public class GetRoomsQuery : IRequest<Response<IEnumerable<RoomDto>>>
    {
        public string? SearchTerm { get; set; }
    }
}
