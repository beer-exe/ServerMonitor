using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Queries.GetUsersByRoomId
{
    public class GetUsersByRoomIdQueryHandler : IRequestHandler<GetUsersByRoomIdQuery, Response<IEnumerable<UserRoomAccessDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetUsersByRoomIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<UserRoomAccessDto>>> Handle(GetUsersByRoomIdQuery request, CancellationToken cancellationToken)
        {
            List<UserRoomAccessDto>? accesses = await _context.UserRoomAccesses
                .AsNoTracking()
                .Include(ura => ura.User)
                .Include(ura => ura.Room)
                .Where(ura => ura.RoomId == request.RoomId)
                .Select(ura => new UserRoomAccessDto
                {
                    UserId = ura.UserId,
                    RoomId = ura.RoomId,
                    UserName = ura.User.Username,
                    RoomName = ura.Room.Name,
                    ReceiveAlerts = ura.ReceiveAlerts,
                    UpdatedAt = ura.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            if (accesses == null || !accesses.Any())
            {
                return new Response<IEnumerable<UserRoomAccessDto>>(Enumerable.Empty<UserRoomAccessDto>(), "Không tìm thấy nhân viên phụ trách quản lý nào.");
            }

            return new Response<IEnumerable<UserRoomAccessDto>>(accesses, "Lấy danh sách nhân viên quản lý phòng thành công.");
        }
    }
}