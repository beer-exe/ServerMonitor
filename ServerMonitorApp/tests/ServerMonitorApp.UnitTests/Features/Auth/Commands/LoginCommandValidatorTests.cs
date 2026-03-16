using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Auth.Commands.Login;

namespace ServerMonitorApp.UnitTests.Features.Auth.Commands
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _validator;

        public LoginCommandValidatorTests()
        {
            _validator = new LoginCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "admin",
                Password = "Password123!"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Validate_EmptyUsernameOrEmail_ReturnsFalseAndError(string usernameOrEmail)
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = usernameOrEmail,
                Password = "Password123!"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tài khoản hoặc Email không được để trống.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Validate_EmptyPassword_ReturnsFalseAndError(string password)
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "admin",
                Password = password
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mật khẩu không được để trống.");
        }

        [Fact]
        public async Task Validate_EmptyBoth_ReturnsMultipleErrors()
        {
            LoginCommand? command = new LoginCommand
            {
                UsernameOrEmail = "",
                Password = null
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Equal(2, result.Errors.Count);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tài khoản hoặc Email không được để trống.");
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mật khẩu không được để trống.");
        }
    }
}
