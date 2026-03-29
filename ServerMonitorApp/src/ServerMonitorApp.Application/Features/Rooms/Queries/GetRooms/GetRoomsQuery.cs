using MediatR;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Rooms.Queries.GetRooms
{
    public class GetRoomsQuery : IRequest<Response<IEnumerable<RoomDto>>>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public string? Role { get; set; } = null!;

        public string? SearchTerm { get; set; }
    }
}
