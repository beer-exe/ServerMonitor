using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess
{
    public class UpdateRoomAccessCommandHandler : IRequestHandler<UpdateRoomAccessCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateRoomAccessCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<string>> Handle(UpdateRoomAccessCommand request, CancellationToken cancellationToken)
        {
            UserRoomAccess? access = await _context.UserRoomAccesses.FirstOrDefaultAsync(ura => ura.UserId == request.UserId && ura.RoomId == request.RoomId, cancellationToken);

            if (access == null)
            {
                throw new ApiException("Không tìm thấy thông tin phân quyền cho người dùng và phòng này.");
            }

            access.ReceiveAlerts = request.ReceiveAlerts;
            access.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>("Thành công", "Cập nhật quyền giám sát thành công.");
        }
    }
}