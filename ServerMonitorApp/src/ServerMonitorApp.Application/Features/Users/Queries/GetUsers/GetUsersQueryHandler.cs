using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Response<IEnumerable<UserDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUsersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<UserDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            List<UserDto>? users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<UserDto>>(users, "Lấy danh sách người dùng thành công.");
        }
    }
}
