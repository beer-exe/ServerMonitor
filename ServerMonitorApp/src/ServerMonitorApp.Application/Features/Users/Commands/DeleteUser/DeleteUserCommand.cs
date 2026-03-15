using MediatR;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<Response<Guid>>
    {
        public Guid Id { get; set; }

        public DeleteUserCommand(Guid id)
        {
            Id = id;
        }
    }
}
