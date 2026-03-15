using MediatR;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<Response<UserDto>>
    {
        public Guid Id { get; set; }

        public GetUserByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
