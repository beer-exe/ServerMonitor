using FluentValidation;

namespace ServerMonitorApp.Application.Features.AccessControl.Commands.AssignRoomAccess
{
    public class AssignRoomAccessCommandValidator : AbstractValidator<AssignRoomAccessCommand>
    {
        public AssignRoomAccessCommandValidator()
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