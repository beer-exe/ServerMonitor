using MediatR;
using ServerMonitorApp.Application.Wrappers;
using System.Text.Json.Serialization;

namespace ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert
{
    public class ResolveAlertCommand : IRequest<Response<long>>
    {
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public Guid UserId { get; set; }
        [JsonIgnore]
        public string? Role { get; set; } = null!;

        public string ResolutionNote { get; set; } = null!;
    }
}