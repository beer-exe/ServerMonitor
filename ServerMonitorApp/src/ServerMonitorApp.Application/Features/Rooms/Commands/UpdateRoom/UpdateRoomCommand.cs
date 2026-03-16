using MediatR;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommand : IRequest<Response<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Location { get; set; }
    }
}
