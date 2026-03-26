using MediatR;
using Microsoft.EntityFrameworkCore;
using ServerMonitorApp.Application.Common.Exceptions;
using ServerMonitorApp.Application.Common.Interfaces;
using ServerMonitorApp.Application.Wrappers;

namespace ServerMonitorApp.Application.Features.Devices.Commands.DeleteDevice
{
    public class DeleteDeviceCommandHandler : IRequestHandler<DeleteDeviceCommand, Response<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteDeviceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Response<bool>> Handle(DeleteDeviceCommand request, CancellationToken cancellationToken)
        {
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);
            if (device == null)
            {
                throw new ApiException("Thiết bị không tồn tại.");
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync(cancellationToken);

            return new Response<bool>(true, "Xóa thiết bị thành công.");
        }
    }
}