using MediatR;
using ServerMonitorApp.Application.Features.Dashboard.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Dashboard.Queries.GetDashboard
{
    public class GetDashboardQuery : IRequest<Response<IEnumerable<DashboardRoomDto>>>
    {
        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
    }
}