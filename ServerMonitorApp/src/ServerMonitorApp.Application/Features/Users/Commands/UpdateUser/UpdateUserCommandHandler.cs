using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Response<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateUserCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<Guid>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

            if (user == null)
            {
                throw new ApiException("Người dùng không tồn tại.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != request.Id, cancellationToken))
            {
                throw new ApiException($"Email '{request.Email}' đã được sử dụng bởi một tài khoản khác.");
            }

            user.Email = request.Email;
            user.Role = request.Role;
            user.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync(cancellationToken);

            return new Response<Guid>(user.Id, "Cập nhật thông tin người dùng thành công.");
        }
    }
}
