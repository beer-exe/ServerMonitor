using MediatR;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Alerts.Queries.GetAlertById
{
    public class GetAlertByIdQuery : IRequest<Response<AlertDto>>
    {
        public long Id { get; set; }
        public Guid UserId { get; set; }
        public string Role { get; set; } = null!;
    }
}