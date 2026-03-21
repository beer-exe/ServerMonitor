using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Alerts.Queries.GetAlerts
{
    public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, PagedResponse<IEnumerable<AlertDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAlertsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<IEnumerable<AlertDto>>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Alert> query = _context.Alerts
                .Include(a => a.Room)
                .Include(a => a.Device)
                .AsNoTracking();

            if (request.Role != "ADMIN")
            {
                query = query.Where(a => _context.UserRoomAccesses.Any(ura => ura.UserId == request.UserId && ura.RoomId == a.RoomId));
            }

            if (request.RoomId.HasValue)
            {
                query = query.Where(a => a.RoomId == request.RoomId.Value);
            }    

            if (!string.IsNullOrEmpty(request.Severity))
            {
                query = query.Where(a => a.Severity == request.Severity);
            }    

            if (request.IsResolved.HasValue)
            {
                query = query.Where(a => a.IsResolved == request.IsResolved.Value);
            }    

            int totalRecords = await query.CountAsync(cancellationToken);

            IEnumerable<AlertDto> alerts = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new AlertDto
                {
                    Id = a.Id,
                    RoomId = a.RoomId,
                    RoomName = a.Room != null ? a.Room.Name : null,
                    DeviceId = a.DeviceId,
                    DeviceName = a.Device != null ? a.Device.Name : null,
                    Message = a.Message,
                    Severity = a.Severity,
                    IsResolved = a.IsResolved,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<IEnumerable<AlertDto>>(
                alerts,
                request.PageNumber,
                request.PageSize,
                totalRecords,
                "Lấy danh sách cảnh báo thành công.");
        }
    }
}