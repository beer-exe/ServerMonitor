using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Features.Rooms.DTOs;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Rooms.Queries.GetRoomById
{
    public class GetRoomByIdQueryHandler : IRequestHandler<GetRoomByIdQuery, Response<RoomDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetRoomByIdQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<RoomDto>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
        {
            RoomDto? room = await _context.Rooms
                .AsNoTracking()
                .Where(r => r.Id == request.Id)
                .Select(r => new RoomDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Location = r.Location,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (room == null)
            {
                throw new ApiException("Phòng không tồn tại.");
            }

            return new Response<RoomDto>(room, "Lấy thông tin phòng thành công.");
        }
    }
}
