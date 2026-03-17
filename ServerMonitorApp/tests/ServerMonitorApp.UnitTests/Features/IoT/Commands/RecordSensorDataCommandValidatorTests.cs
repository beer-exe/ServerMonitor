using FluentValidation.Results;
using ServerMonitorApp.Application.Features.IoT.Commands.RecordSensorData;

namespace ServerMonitorApp.UnitTests.Features.IoT.Commands
{
    public class RecordSensorDataCommandValidatorTests
    {
        private readonly RecordSensorDataCommandValidator _validator;

        public RecordSensorDataCommandValidatorTests()
        {
            _validator = new RecordSensorDataCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                DeviceId = Guid.NewGuid(),
                Temperature = 25.0m,
                Humidity = 50.0m
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_EmptyDeviceId_ReturnsFalseAndError()
        {
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                DeviceId = Guid.Empty,
                Temperature = 25.0m,
                Humidity = 50.0m
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã thiết bị không được để trống.");
        }

        [Theory]
        [InlineData(-51.0)]
        [InlineData(101.0)]
        public async Task Validate_TemperatureOutOfRange_ReturnsFalseAndError(double invalidTemp)
        {
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                DeviceId = Guid.NewGuid(),
                Temperature = (decimal)invalidTemp,
                Humidity = 50.0m
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Nhiệt độ phải nằm trong khoảng từ -50 đến 100 độ C.");
        }

        [Theory]
        [InlineData(-1.0)]
        [InlineData(101.0)]
        public async Task Validate_HumidityOutOfRange_ReturnsFalseAndError(double invalidHumidity)
        {
            RecordSensorDataCommand? command = new RecordSensorDataCommand
            {
                DeviceId = Guid.NewGuid(),
                Temperature = 25.0m,
                Humidity = (decimal)invalidHumidity
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Độ ẩm phải nằm trong khoảng từ 0% đến 100%.");
        }
    }
}