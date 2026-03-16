using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.RevokeRoomAccess
{
    public class RevokeRoomAccessCommandHandler : IRequestHandler<RevokeRoomAccessCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;

        public RevokeRoomAccessCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<string>> Handle(RevokeRoomAccessCommand request, CancellationToken cancellationToken)
        {
            UserRoomAccess? access = await _context.UserRoomAccesses.FirstOrDefaultAsync(ura => ura.UserId == request.UserId && ura.RoomId == request.RoomId, cancellationToken);

            if (access == null)
            {
                throw new ApiException("Không tìm thấy thông tin phân quyền để thu hồi.");
            }

            _context.UserRoomAccesses.Remove(access);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>("Thành công", "Đã thu hồi quyền giám sát phòng của người dùng.");
        }
    }
}