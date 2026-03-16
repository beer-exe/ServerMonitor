using FluentValidation;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess
{
    public class UpdateRoomAccessCommandValidator : AbstractValidator<UpdateRoomAccessCommand>
    {
        public UpdateRoomAccessCommandValidator()
        {
            RuleFor(p => p.UserId)
                .NotEmpty().WithMessage("Mã người dùng (UserId) không được để trống.");
            RuleFor(p => p.RoomId)
                .NotEmpty().WithMessage("Mã phòng (RoomId) không được để trống.");
            RuleFor(p => p.ReceiveAlerts)
                .NotNull().WithMessage("Trạng thái nhận cảnh báo (ReceiveAlerts) không được để trống.");
        }
    }
}