using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Users.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Response<UserDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            UserDto? user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == request.Id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                throw new ApiException("Người dùng không tồn tại.");
            }

            return new Response<UserDto>(user, "Lấy thông tin người dùng thành công.");
        }
    }
}
