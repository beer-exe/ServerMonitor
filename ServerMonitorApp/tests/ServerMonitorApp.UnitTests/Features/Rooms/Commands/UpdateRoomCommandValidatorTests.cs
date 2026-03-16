using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Rooms.Commands.UpdateRoom;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Commands
{
    public class UpdateRoomCommandValidatorTests
    {
        private readonly UpdateRoomCommandValidator _validator;

        public UpdateRoomCommandValidatorTests()
        {
            _validator = new UpdateRoomCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = Guid.NewGuid(),
                Name = "Phòng Backup",
                Location = "Tầng 2 - Tòa B"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_EmptyName_ReturnsFalseAndError(string name)
        {
            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = Guid.NewGuid(),
                Name = name,
                Location = "Tầng 2"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tên phòng không được để trống.");
        }

        [Fact]
        public async Task Validate_NameExceedsMaxLength_ReturnsFalseAndError()
        {
            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = Guid.NewGuid(),
                Name = new string('X', 101),
                Location = "Tầng 2"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tên phòng không được vượt quá 100 ký tự.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_EmptyLocation_ReturnsFalseAndError(string location)
        {
            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = Guid.NewGuid(),
                Name = "Phòng Data",
                Location = location
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Vị trí phòng không được để trống.");
        }

        [Fact]
        public async Task Validate_LocationExceedsMaxLength_ReturnsFalseAndError()
        {
            UpdateRoomCommand? command = new UpdateRoomCommand
            {
                Id = Guid.NewGuid(),
                Name = "Phòng Data",
                Location = new string('Y', 256)
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Vị trí phòng không được vượt quá 255 ký tự.");
        }
    }
}
