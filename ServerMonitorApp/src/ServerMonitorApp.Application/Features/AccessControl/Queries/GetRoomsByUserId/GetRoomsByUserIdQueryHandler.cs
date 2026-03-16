using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.AccessControl.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.AccessControl.Queries.GetRoomsByUserId
{
    public class GetRoomsByUserIdQueryHandler : IRequestHandler<GetRoomsByUserIdQuery, Response<IEnumerable<UserRoomAccessDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetRoomsByUserIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<UserRoomAccessDto>>> Handle(GetRoomsByUserIdQuery request, CancellationToken cancellationToken)
        {
            IEnumerable<UserRoomAccessDto>? accesses = await _context.UserRoomAccesses
                .AsNoTracking()
                .Include(ura => ura.Room)
                .Include(ura => ura.User)
                .Where(ura => ura.UserId == request.UserId)
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
                return new Response<IEnumerable<UserRoomAccessDto>>(Enumerable.Empty<UserRoomAccessDto>(), "Không tìm thấy phòng nào được phân quyền cho nhân viên này.");
            }

            return new Response<IEnumerable<UserRoomAccessDto>>(accesses, "Lấy danh sách phòng được phân quyền thành công.");
        }
    }
}