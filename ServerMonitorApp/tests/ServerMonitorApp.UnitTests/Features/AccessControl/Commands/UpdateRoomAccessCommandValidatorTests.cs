using FluentValidation.Results;
using ServerMonitorApp.Application.Features.AccessControl.Commands.UpdateRoomAccess;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Commands
{
    public class UpdateRoomAccessCommandValidatorTests
    {
        private readonly UpdateRoomAccessCommandValidator _validator;

        public UpdateRoomAccessCommandValidatorTests()
        {
            _validator = new UpdateRoomAccessCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            UpdateRoomAccessCommand? command = new UpdateRoomAccessCommand
            {
                UserId = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                ReceiveAlerts = true
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_EmptyUserId_ReturnsFalseAndError()
        {
            UpdateRoomAccessCommand? command = new UpdateRoomAccessCommand
            {
                UserId = Guid.Empty,
                RoomId = Guid.NewGuid(),
                ReceiveAlerts = true
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã người dùng (UserId) không được để trống.");
        }

        [Fact]
        public async Task Validate_EmptyRoomId_ReturnsFalseAndError()
        {
            UpdateRoomAccessCommand? command = new UpdateRoomAccessCommand
            {
                UserId = Guid.NewGuid(),
                RoomId = Guid.Empty,
                ReceiveAlerts = true
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã phòng (RoomId) không được để trống.");
        }
    }
}