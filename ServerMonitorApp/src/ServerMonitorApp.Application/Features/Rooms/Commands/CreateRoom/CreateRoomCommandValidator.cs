using FluentValidation;

namespace ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom
{
    public class CreateRoomCommandValidator : AbstractValidator<CreateRoomCommand>
    {
        public CreateRoomCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên phòng không được để trống.")
                .MaximumLength(100).WithMessage("Tên phòng không được vượt quá 100 ký tự.");
            RuleFor(p => p.Location)
                .NotEmpty().WithMessage("Vị trí phòng không được để trống.")
                .MaximumLength(255).WithMessage("Vị trí phòng không được vượt quá 255 ký tự.");
        }
    }
}
