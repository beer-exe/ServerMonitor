using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommand : IRequest<Response<Guid>>
    {
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
