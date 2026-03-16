using FluentValidation.Results;
using ServerMonitorApp.Application.Features.AccessControl.Commands.RevokeRoomAccess;

namespace ServerMonitorApp.UnitTests.Features.AccessControl.Commands
{
    public class RevokeRoomAccessCommandValidatorTests
    {
        private readonly RevokeRoomAccessCommandValidator _validator;

        public RevokeRoomAccessCommandValidatorTests()
        {
            _validator = new RevokeRoomAccessCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            RevokeRoomAccessCommand? command = new RevokeRoomAccessCommand(Guid.NewGuid(), Guid.NewGuid());
            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Fact]
        public async Task Validate_EmptyUserId_ReturnsFalseAndError()
        {
            RevokeRoomAccessCommand? command = new RevokeRoomAccessCommand(Guid.Empty, Guid.NewGuid());
            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã người dùng (UserId) không được để trống.");
        }

        [Fact]
        public async Task Validate_EmptyRoomId_ReturnsFalseAndError()
        {
            RevokeRoomAccessCommand? command = new RevokeRoomAccessCommand(Guid.NewGuid(), Guid.Empty);
            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã phòng (RoomId) không được để trống.");
        }
    }
}