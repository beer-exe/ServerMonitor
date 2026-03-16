using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.AssignRoomAccess
{
    public class AssignRoomAccessCommandHandler : IRequestHandler<AssignRoomAccessCommand, Response<string>>
    {
        private readonly IApplicationDbContext _context;

        public AssignRoomAccessCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<string>> Handle(AssignRoomAccessCommand request, CancellationToken cancellationToken)
        {
            bool userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
            if (!userExists)
            {
                throw new ApiException("Mã nhân viên không tồn tại không tồn tại.");
            }

            bool roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId, cancellationToken);
            if (!roomExists)
            {
                throw new ApiException("Mã phòng không tồn tại.");
            }

            bool accessExists = await _context.UserRoomAccesses.AnyAsync(ura => ura.UserId == request.UserId && ura.RoomId == request.RoomId, cancellationToken);
            if (accessExists)
            {
                throw new ApiException("Nhân viên này đã được phân quyền quản lý phòng này trước đó.");
            }

            UserRoomAccess? access = new UserRoomAccess
            {
                UserId = request.UserId,
                RoomId = request.RoomId,
                ReceiveAlerts = request.ReceiveAlerts,
                UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.UserRoomAccesses.Add(access);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<string>("Thành công", "Cấp quyền giám sát phòng thành công.");
        }
    }
}