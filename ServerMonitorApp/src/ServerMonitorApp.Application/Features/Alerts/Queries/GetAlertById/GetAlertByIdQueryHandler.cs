using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.Queries.GetAlertById
{
    public class GetAlertByIdQueryHandler : IRequestHandler<GetAlertByIdQuery, Response<AlertDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAlertByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<AlertDto>> Handle(GetAlertByIdQuery request, CancellationToken cancellationToken)
        {
            Alert? alert = await _context.Alerts
                .Include(a => a.Room)
                .Include(a => a.Device)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (alert == null)
            {
                throw new ApiException("Không tìm thấy cảnh báo.");
            }    

            if (request.Role != "ADMIN")
            {
                bool hasAccess = await _context.UserRoomAccesses.AnyAsync(ura => ura.UserId == request.UserId && ura.RoomId == alert.RoomId, cancellationToken);

                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem cảnh báo của phòng này.");
                }    
            }

            AlertDto? dto = new AlertDto
            {
                Id = alert.Id,
                RoomId = alert.RoomId,
                RoomName = alert.Room?.Name,
                DeviceId = alert.DeviceId,
                DeviceName = alert.Device?.Name,
                Message = alert.Message,
                Severity = alert.Severity,
                IsResolved = alert.IsResolved,
                CreatedAt = alert.CreatedAt,
                UpdatedAt = alert.UpdatedAt
            };

            return new Response<AlertDto>(dto, "Lấy thông tin cảnh báo thành công.");
        }
    }
}