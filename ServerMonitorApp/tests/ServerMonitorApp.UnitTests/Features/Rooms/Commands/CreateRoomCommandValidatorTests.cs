using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Rooms.Commands.CreateRoom;

namespace ServerMonitorApp.UnitTests.Features.Rooms.Commands
{
    public class CreateRoomCommandValidatorTests
    {
        private readonly CreateRoomCommandValidator _validator;

        public CreateRoomCommandValidatorTests()
        {
            _validator = new CreateRoomCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = "Phòng Server Chính",
                Location = "Tầng 1 - Tòa A"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_EmptyName_ReturnsFalseAndError(string name)
        {
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = name,
                Location = "Tầng 1"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tên phòng không được để trống.");
        }

        [Fact]
        public async Task Validate_NameExceedsMaxLength_ReturnsFalseAndError()
        {
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = new string('A', 101),
                Location = "Tầng 1"
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
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = "Phòng Server",
                Location = location
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Vị trí phòng không được để trống.");
        }

        [Fact]
        public async Task Validate_LocationExceedsMaxLength_ReturnsFalseAndError()
        {
            CreateRoomCommand? command = new CreateRoomCommand
            {
                Name = "Phòng Server",
                Location = new string('B', 256)
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Vị trí phòng không được vượt quá 255 ký tự.");
        }
    }
}
