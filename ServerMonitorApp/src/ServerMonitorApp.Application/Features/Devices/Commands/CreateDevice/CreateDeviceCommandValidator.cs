using FluentValidation;

namespace ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice
{
    public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
    {
        public CreateDeviceCommandValidator()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Tên thiết bị là bắt buộc.")
                .MaximumLength(200).WithMessage("Tên thiết bị không được vượt quá 200 ký tự.");

            RuleFor(p => p.TemperatureWarningThreshold)
                .LessThan(p => p.TemperatureCriticalThreshold)
                .When(p => p.TemperatureWarningThreshold.HasValue && p.TemperatureCriticalThreshold.HasValue)
                .WithMessage("Ngưỡng cảnh báo nhiệt độ phải nhỏ hơn ngưỡng nhiệt độ nguy hiểm.");

            RuleFor(p => p.HumidityWarningThreshold)
                .LessThan(p => p.HumidityCriticalThreshold)
                .When(p => p.HumidityWarningThreshold.HasValue && p.HumidityCriticalThreshold.HasValue)
                .WithMessage("Ngưỡng cảnh báo độ ẩm phải nhỏ hơn ngưỡng độ ẩm nguy hiểm.");

            RuleFor(p => p.HumidityWarningThreshold)
                .InclusiveBetween(0, 100)
                .When(p => p.HumidityWarningThreshold.HasValue)
                .WithMessage("Độ ẩm cảnh báo phải nằm trong khoảng từ 0% đến 100%.");

            RuleFor(p => p.HumidityCriticalThreshold)
                .InclusiveBetween(0, 100)
                .When(p => p.HumidityCriticalThreshold.HasValue)
                .WithMessage("Độ ẩm nguy hiểm phải nằm trong khoảng từ 0% đến 100%.");
        }
    }
}