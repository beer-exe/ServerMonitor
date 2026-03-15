using FluentValidation;

namespace ServerMonitorApp.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng Email không hợp lệ.");

            RuleFor(p => p.Role)
                .NotEmpty().WithMessage("Quyền không được để trống.")
                .Must(role => role == "ADMIN" || role == "USER").WithMessage("Quyền chỉ được phép là 'ADMIN' hoặc 'USER'.");
        }
    }
}
