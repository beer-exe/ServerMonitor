using FluentValidation;

namespace ServerMonitorApp.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(p => p.UsernameOrEmail)
                .NotEmpty().WithMessage("Tài khoản hoặc Email không được để trống.");

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.");
        }
    }
}
