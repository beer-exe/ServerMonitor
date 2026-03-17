using FluentValidation;

namespace ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData
{
    public class RecordSensorDataCommandValidator : AbstractValidator<RecordSensorDataCommand>
    {
        public RecordSensorDataCommandValidator()
        {
            RuleFor(p => p.DeviceId)
                .NotEmpty().WithMessage("Mã thiết bị không được để trống.");

            RuleFor(p => p.Temperature)
                .InclusiveBetween(-50, 100).WithMessage("Nhiệt độ phải nằm trong khoảng từ -50 đến 100 độ C.");

            RuleFor(p => p.Humidity)
                .InclusiveBetween(0, 100).WithMessage("Độ ẩm phải nằm trong khoảng từ 0% đến 100%.");
        }
    }
}