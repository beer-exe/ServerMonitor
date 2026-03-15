using MediatR;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommand : IRequest<Response<Guid>>
    {
        [JsonIgnore]
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
