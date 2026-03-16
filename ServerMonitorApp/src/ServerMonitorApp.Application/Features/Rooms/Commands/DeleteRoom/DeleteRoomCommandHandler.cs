using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.DeleteRoom
{
    public class DeleteRoomCommandHandler : IRequestHandler<DeleteRoomCommand, Response<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteRoomCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<Guid>> Handle(DeleteRoomCommand request, CancellationToken cancellationToken)
        {
            Room? room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (room == null)
            {
                throw new ApiException("Phòng không tồn tại.");
            }

            bool hasUnresolvedAlerts = await _context.Alerts.AnyAsync(a => a.RoomId == request.Id && (a.IsResolved == false || a.IsResolved == null), cancellationToken);
            if (hasUnresolvedAlerts)
            {
                throw new ApiException("Không thể xóa phòng đang có cảnh báo chưa được xử lý. Vui lòng xử lý tất cả cảnh báo trước.");
            }

            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<Guid>(room.Id, "Xóa phòng thành công.");
        }
    }
}
