using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Devices.Commands.UpdateDevice;
using System;
using Xunit;

namespace ServerMonitorApp.UnitTests.Features.Devices.Commands
{
    public class UpdateDeviceCommandValidatorTests
    {
        private readonly UpdateDeviceCommandValidator _validator;

        public UpdateDeviceCommandValidatorTests()
        {
            _validator = new UpdateDeviceCommandValidator();
        }

        [Fact]
        public void Validate_GivenValidCommand_ReturnsIsValid()
        {
            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = Guid.NewGuid(),
                Name = "Sensor Cập nhật",
                TemperatureWarningThreshold = 30,
                TemperatureCriticalThreshold = 40,
                HumidityWarningThreshold = 60,
                HumidityCriticalThreshold = 80
            };

            ValidationResult? result = _validator.Validate(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_EmptyId_ReturnsError()
        {
            UpdateDeviceCommand? command = new UpdateDeviceCommand { Id = Guid.Empty, Name = "Sensor 1" };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Id" && e.ErrorMessage == "ID thiết bị là bắt buộc.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void Validate_EmptyName_ReturnsError(string name)
        {
            UpdateDeviceCommand? command = new UpdateDeviceCommand { Id = Guid.NewGuid(), Name = name };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Tên thiết bị là bắt buộc.");
        }

        [Fact]
        public void Validate_NameExceedsMaxLength_ReturnsError()
        {
            UpdateDeviceCommand? command = new UpdateDeviceCommand { Id = Guid.NewGuid(), Name = new string('A', 201) };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage == "Tên thiết bị không được vượt quá 200 ký tự.");
        }

        [Fact]
        public void Validate_TempWarningGreaterThanCritical_ReturnsError()
        {
            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = Guid.NewGuid(),
                Name = "Sensor 1",
                TemperatureWarningThreshold = 50,
                TemperatureCriticalThreshold = 40
            };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "TemperatureWarningThreshold" && e.ErrorMessage == "Ngưỡng cảnh báo nhiệt độ phải nhỏ hơn ngưỡng nhiệt độ nguy hiểm.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Validate_HumidityWarningOutOfRange_ReturnsError(decimal invalidHumidity)
        {
            UpdateDeviceCommand? command = new UpdateDeviceCommand
            {
                Id = Guid.NewGuid(),
                Name = "Sensor 1",
                HumidityWarningThreshold = invalidHumidity
            };

            ValidationResult? result = _validator.Validate(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "HumidityWarningThreshold" && e.ErrorMessage == "Độ ẩm cảnh báo phải nằm trong khoảng từ 0% đến 100%.");
        }
    }
}