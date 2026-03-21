using MediatR;
using ServerMonitorApp.Application.Features.Alerts.DTOs;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Alerts.Queries.GetAlerts
{
    public class GetAlertsQuery : IRequest<PagedResponse<IEnumerable<AlertDto>>>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public string Role { get; set; } = null!;

        public Guid? RoomId { get; set; }
        public string? Severity { get; set; }
        public bool? IsResolved { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}