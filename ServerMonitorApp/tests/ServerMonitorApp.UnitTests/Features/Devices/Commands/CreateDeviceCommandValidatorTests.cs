using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Devices.Commands.CreateDevice;
using Xunit;

namespace ServerMonitorApp.UnitTests.Features.Devices.Commands
{
    public class CreateDeviceCommandValidatorTests
    {
        private readonly CreateDeviceCommandValidator _validator;

        public CreateDeviceCommandValidatorTests()
        {
            _validator = new CreateDeviceCommandValidator();
        }

        [Fact]
        public void Validate_GivenValidCommand_ReturnsIsValid()
        {
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Sensor Nhiệt độ 1",
                TemperatureWarningThreshold = 30,
                TemperatureCriticalThreshold = 40,
                HumidityWarningThreshold = 60,
                HumidityCriticalThreshold = 80
            };

            ValidationResult? result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyName_ReturnsError(string name)
        {
            CreateDeviceCommand? command = new CreateDeviceCommand { Name = name };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Tên thiết bị là bắt buộc.");
        }

        [Fact]
        public void Validate_NameExceedsMaxLength_ReturnsError()
        {
            CreateDeviceCommand? command = new CreateDeviceCommand { Name = new string('A', 201) };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Tên thiết bị không được vượt quá 200 ký tự.");
        }

        [Fact]
        public void Validate_TempWarningGreaterThanCritical_ReturnsError()
        {
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Sensor 1",
                TemperatureWarningThreshold = 50,
                TemperatureCriticalThreshold = 40
            };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "TemperatureWarningThreshold");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_HumidityWarningOutOfRange_ReturnsError(decimal invalidHumidity)
        {
            CreateDeviceCommand? command = new CreateDeviceCommand
            {
                Name = "Sensor 1",
                HumidityWarningThreshold = invalidHumidity
            };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "HumidityWarningThreshold" && e.ErrorMessage == "Độ ẩm cảnh báo phải nằm trong khoảng từ 0% đến 100%.");
        }
    }
}