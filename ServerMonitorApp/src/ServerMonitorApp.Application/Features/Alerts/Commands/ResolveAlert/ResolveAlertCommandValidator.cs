using FluentValidation;

namespace ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert
{
    public class ResolveAlertCommandValidator : AbstractValidator<ResolveAlertCommand>
    {
        public ResolveAlertCommandValidator()
        {
            RuleFor(p => p.Id)
                .GreaterThan(0).WithMessage("Mã cảnh báo (Id) không hợp lệ.");

            RuleFor(p => p.ResolutionNote)
                .NotEmpty().WithMessage("Ghi chú xử lý (ResolutionNote) không được để trống.")
                .MaximumLength(1000).WithMessage("Ghi chú xử lý không được vượt quá 1000 ký tự.");
        }
    }
}