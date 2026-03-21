using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert
{
    public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, Response<long>>
    {
        private readonly IApplicationDbContext _context;

        public ResolveAlertCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<long>> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
        {
            Alert? alert = await _context.Alerts.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (alert == null)
            {
                throw new ApiException("Không tìm thấy cảnh báo.");
            }    

            if (request.Role != "ADMIN")
            {
                bool hasAccess = await _context.UserRoomAccesses
                    .AnyAsync(ura => ura.UserId == request.UserId && ura.RoomId == alert.RoomId, cancellationToken);

                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xử lý cảnh báo của phòng này.");
                }    
            }

            if (alert.IsResolved == true)
            {
                throw new ApiException("Cảnh báo này đã được xử lý trước đó.");
            }    

            alert.IsResolved = true;
            alert.Message += $"\n[Đã xử lý]: {request.ResolutionNote}";
            alert.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<long>(alert.Id, "Đã cập nhật trạng thái xử lý sự cố thành công.");
        }
    }
}