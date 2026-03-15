using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;
using ServerMonitorApp.Domain.Models;

namespace ServerMonitorApp.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Response<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Response<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username, cancellationToken))
            {
                throw new ApiException($"Tên đăng nhập '{request.Username}' đã tồn tại.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email, cancellationToken))
            {
                throw new ApiException($"Email '{request.Email}' đã được sử dụng.");
            }

            User? user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                Role = request.Role,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<Guid>(user.Id, "Tạo người dùng thành công.");
        }
    }
}
