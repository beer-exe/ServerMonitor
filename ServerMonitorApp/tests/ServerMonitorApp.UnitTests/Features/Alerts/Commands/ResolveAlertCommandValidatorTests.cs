using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Alerts.Commands.ResolveAlert;

namespace ServerMonitorApp.UnitTests.Features.Alerts.Commands
{
    public class ResolveAlertCommandValidatorTests
    {
        private readonly ResolveAlertCommandValidator _validator;

        public ResolveAlertCommandValidatorTests()
        {
            _validator = new ResolveAlertCommandValidator();
        }

        [Fact]
        public async Task Validate_ValidCommand_ReturnsTrue()
        {
            ResolveAlertCommand? command = new ResolveAlertCommand
            {
                Id = 1,
                UserId = Guid.NewGuid(),
                Role = "ADMIN",
                ResolutionNote = "Đã thay thế cáp mạng bị hỏng và khôi phục kết nối."
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public async Task Validate_InvalidId_ReturnsFalseAndError(long invalidId)
        {
            ResolveAlertCommand? command = new ResolveAlertCommand
            {
                Id = invalidId,
                UserId = Guid.NewGuid(),
                Role = "ADMIN",
                ResolutionNote = "Đã xử lý xong."
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã cảnh báo (Id) không hợp lệ.");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Validate_EmptyResolutionNote_ReturnsFalseAndError(string invalidNote)
        {
            ResolveAlertCommand? command = new ResolveAlertCommand
            {
                Id = 10,
                UserId = Guid.NewGuid(),
                Role = "USER",
                ResolutionNote = invalidNote
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Ghi chú xử lý (ResolutionNote) không được để trống.");
        }

        [Fact]
        public async Task Validate_ResolutionNoteExceedsMaxLength_ReturnsFalseAndError()
        {
            ResolveAlertCommand? command = new ResolveAlertCommand
            {
                Id = 5,
                UserId = Guid.NewGuid(),
                Role = "ADMIN",
                ResolutionNote = new string('A', 1001)
            };

            ValidationResult? result = await _validator.ValidateAsync(command);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Ghi chú xử lý không được vượt quá 1000 ký tự.");
        }
    }
}