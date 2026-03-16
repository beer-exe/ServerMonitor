using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Users.Commands.UpdateUser;

namespace ServerMonitorApp.UnitTests.Features.Users.Commands
{
    public class UpdateUserCommandValidatorTests
    {
        private readonly UpdateUserCommandValidator _validator;

        public UpdateUserCommandValidatorTests()
        {
            _validator = new UpdateUserCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            UpdateUserCommand? command = new UpdateUserCommand
            {
                Id = Guid.NewGuid(),
                Email = "updated@example.com",
                Role = "ADMIN"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid-email-format")]
        [InlineData(null)]
        public async Task Validate_InvalidEmail_ReturnsFalseAndError(string email)
        {
            UpdateUserCommand? command = new UpdateUserCommand
            {
                Id = Guid.NewGuid(),
                Email = email,
                Role = "USER"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        }

        [Theory]
        [InlineData("MANAGER")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_InvalidRole_ReturnsFalseAndError(string role)
        {
            UpdateUserCommand? command = new UpdateUserCommand
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                Role = role
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Quyền chỉ được phép là 'ADMIN' hoặc 'USER'." || e.ErrorMessage == "Quyền không được để trống.");
        }
    }
}