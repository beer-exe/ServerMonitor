using FluentValidation;

namespace ServerMonitorApp.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        public CreateUserCommandValidator()
        {
            RuleFor(p => p.Username)
                .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
                .MaximumLength(50).WithMessage("Tên đăng nhập không được vượt quá 50 ký tự.");

            RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng Email không hợp lệ.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

            RuleFor(p => p.Role)
                .NotEmpty().WithMessage("Quyền không được để trống.")
                .Must(role => role == "ADMIN" || role == "USER").WithMessage("Quyền chỉ được phép là 'ADMIN' hoặc 'USER'.");
        }
    }
}
