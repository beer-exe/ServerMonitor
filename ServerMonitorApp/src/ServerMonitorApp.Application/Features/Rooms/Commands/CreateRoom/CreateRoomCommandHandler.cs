using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, Response<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreateRoomCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<Guid>> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
        {
            bool isDuplicate = await _context.Rooms.AnyAsync(r => r.Name == request.Name && r.Location == request.Location, cancellationToken);
            if (isDuplicate)
            {
                throw new ApiException($"Đã tồn tại phòng có tên '{request.Name}' tại vị trí '{request.Location}'. Vui lòng chọn tên hoặc vị trí khác.");
            }

            Room? room = new Room
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Location = request.Location
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<Guid>(room.Id, "Tạo phòng thành công.");
        }
    }
}
