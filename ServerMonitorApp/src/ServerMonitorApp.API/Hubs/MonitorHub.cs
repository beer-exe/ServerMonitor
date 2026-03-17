using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace ServerMonitorApp.API.Hubs
{
    [Authorize]
    public class MonitorHub : Hub<IMonitorClient>
    {
        private readonly IApplicationDbContext _context;

        public MonitorHub(IApplicationDbContext context)
        {
            _context = context;
        }

        public override async Task OnConnectedAsync()
        {
            string? userIdString = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            string? role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (Guid.TryParse(userIdString, out Guid userId))
            {
                List<Guid> accessibleRoomIds;

                if (role == "ADMIN")
                {
                    accessibleRoomIds = await _context.Rooms.Select(r => r.Id).ToListAsync();
                }
                else
                {
                    accessibleRoomIds = await _context.UserRoomAccesses
                        .Where(ura => ura.UserId == userId)
                        .Select(ura => ura.RoomId)
                        .ToListAsync();
                }

                foreach (Guid roomId in accessibleRoomIds)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Room_{roomId}");
                }
            }

            await base.OnConnectedAsync();
        }
    }
}
