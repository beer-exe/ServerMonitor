using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.UpdateRoom
{
    public class UpdateRoomCommandHandler : IRequestHandler<UpdateRoomCommand, Response<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateRoomCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<Guid>> Handle(UpdateRoomCommand request, CancellationToken cancellationToken)
        {
            Room? room = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

            if (room == null)
            {
                throw new ApiException("Phòng không tồn tại.");
            }

            bool isDuplicate = await _context.Rooms.AnyAsync(r => r.Name == request.Name && r.Location == request.Location, cancellationToken);
            if (isDuplicate)
            {
                throw new ApiException($"Đã tồn tại phòng có tên '{request.Name}' tại vị trí '{request.Location}'. Vui lòng chọn tên hoặc vị trí khác.");
            }

            room.Name = request.Name;
            room.Location = request.Location;
            room.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<Guid>(room.Id, "Cập nhật thông tin phòng thành công.");
        }
    }
}
