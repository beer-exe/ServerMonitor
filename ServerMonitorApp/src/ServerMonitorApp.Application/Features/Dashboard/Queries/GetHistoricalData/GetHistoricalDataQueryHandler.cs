using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Dashboard.Queries.GetHistoricalData
{
    public class GetHistoricalDataQueryHandler : IRequestHandler<GetHistoricalDataQuery, PagedResponse<IEnumerable<ChartDataPointDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetHistoricalDataQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResponse<IEnumerable<ChartDataPointDto>>> Handle(GetHistoricalDataQuery request, CancellationToken cancellationToken)
        {
            Device? device = await _context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == request.DeviceId, cancellationToken);
            if (device == null)
            {
                throw new ApiException("Thiết bị không tồn tại.");
            }

            if (request.Role != "ADMIN")
            {
                bool hasAccess = await _context.UserRoomAccesses.AnyAsync(ura => ura.UserId == request.UserId && ura.RoomId == device.RoomId, cancellationToken);

                if (!hasAccess)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem dữ liệu của thiết bị này.");
                }
            }

            DateTime effectiveStartTime = request.StartTime;
            DateTime effectiveEndTime = request.EndTime;

            if (request.StartTime == request.EndTime)
            {
                effectiveEndTime = request.EndTime.AddMinutes(1).AddTicks(-1);
            }

            IQueryable<SensorData>? query = _context.SensorDatas
                .AsNoTracking()
                .Where(s => s.DeviceId == request.DeviceId && s.Timestamp >= effectiveStartTime && s.Timestamp <= effectiveEndTime);

            int totalRecords = await query.CountAsync(cancellationToken);

            IEnumerable<ChartDataPointDto>? historicalData = await query
                .OrderBy(s => s.Timestamp)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(s => new ChartDataPointDto
                {
                    Timestamp = s.Timestamp,
                    Temperature = s.Temperature,
                    Humidity = s.Humidity
                })
                .ToListAsync(cancellationToken);

            return new PagedResponse<IEnumerable<ChartDataPointDto>>
            (
                historicalData,
                request.PageNumber,
                request.PageSize,
                totalRecords,
                "Truy xuất lịch sử thành công."
            );
        }
    }
}