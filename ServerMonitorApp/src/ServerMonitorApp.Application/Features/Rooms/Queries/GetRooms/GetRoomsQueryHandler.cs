using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Rooms.Queries.GetRooms
{
    public class GetRoomsQueryHandler : IRequestHandler<GetRoomsQuery, Response<IEnumerable<RoomDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetRoomsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<IEnumerable<RoomDto>>> Handle(GetRoomsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Room?> query = _context.Rooms.AsNoTracking().AsQueryable();

            if (request.Role != "ADMIN")
            {
                query = query.Where(r => r.UserRoomAccesses.Any(ura => ura.UserId == request.UserId));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                string? searchTerm = request.SearchTerm.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(searchTerm) || (r.Location != null && r.Location.ToLower().Contains(searchTerm)));
            }

            List<RoomDto>? rooms = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Location = r.Location,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync(cancellationToken);

            return new Response<IEnumerable<RoomDto>>(rooms, "Lấy danh sách phòng thành công.");
        }
    }
}
