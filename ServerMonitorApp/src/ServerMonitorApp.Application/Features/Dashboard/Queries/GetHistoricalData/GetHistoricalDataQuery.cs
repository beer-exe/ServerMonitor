using MediatR;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Dashboard.Queries.GetHistoricalData
{
    public class GetHistoricalDataQuery : IRequest<PagedResponse<IEnumerable<ChartDataPointDto>>>
    {
        public Guid DeviceId { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
