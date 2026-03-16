using FluentValidation;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.RevokeRoomAccess
{
    public class RevokeRoomAccessCommandValidator : AbstractValidator<RevokeRoomAccessCommand>
    {
        public RevokeRoomAccessCommandValidator()
        {
            RuleFor(p => p.UserId)
                .NotEmpty().WithMessage("Mã người dùng (UserId) không được để trống.");
            RuleFor(p => p.RoomId)
                .NotEmpty().WithMessage("Mã phòng (RoomId) không được để trống.");
        }
    }
}
