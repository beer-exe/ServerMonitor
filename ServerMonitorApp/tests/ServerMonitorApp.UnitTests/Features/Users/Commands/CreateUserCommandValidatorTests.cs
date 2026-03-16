using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Users.Commands.CreateUser;

namespace ServerMonitorApp.UnitTests.Features.Users.Commands
{
    public class CreateUserCommandValidatorTests
    {
        private readonly CreateUserCommandValidator _validator;

        public CreateUserCommandValidatorTests()
        {
            _validator = new CreateUserCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            CreateUserCommand? command = new CreateUserCommand
            {
                Username = "validuser",
                Email = "user@example.com",
                Password = "ValidPassword123",
                Role = "USER"
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Validate_EmptyUsername_ReturnsFalseAndError(string username)
        {
            CreateUserCommand? command = new CreateUserCommand 
            { 
                Username = username, 
                Email = "test@test.com", 
                Password = "password", 
                Role = "USER" 
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tên đăng nhập không được để trống.");
        }

        [Fact]
        public async Task Validate_UsernameExceedsMaxLength_ReturnsFalseAndError()
        {
            CreateUserCommand? command = new CreateUserCommand 
            { 
                Username = new string('A', 51), 
                Email = "test@test.com", 
                Password = "password", 
                Role = "USER" 
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Tên đăng nhập không được vượt quá 50 ký tự.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-email")]
        public async Task Validate_InvalidEmail_ReturnsFalseAndError(string email)
        {
            CreateUserCommand? command = new CreateUserCommand 
            { 
                Username = "user", 
                Email = email, 
                Password = "password", 
                Role = "USER" 
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        }

        [Theory]
        [InlineData("12345")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Validate_InvalidPassword_ReturnsFalseAndError(string password)
        {
            CreateUserCommand? command = new CreateUserCommand 
            { 
                Username = "user", 
                Email = "test@test.com", 
                Password = password, 
                Role = "USER" 
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        }

        [Theory]
        [InlineData("GUEST")]
        [InlineData("SUPERADMIN")]
        [InlineData("")]
        public async Task Validate_InvalidRole_ReturnsFalseAndError(string role)
        {
            CreateUserCommand? command = new CreateUserCommand 
            { 
                Username = "user", 
                Email = "test@test.com", 
                Password = "password123", 
                Role = role 
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Quyền chỉ được phép là 'ADMIN' hoặc 'USER'." || e.ErrorMessage == "Quyền không được để trống.");
        }
    }
}