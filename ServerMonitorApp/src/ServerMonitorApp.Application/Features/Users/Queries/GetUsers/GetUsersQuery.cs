using MediatR;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQuery : IRequest<Response<IEnumerable<UserDto>>>
    {
    }
}
