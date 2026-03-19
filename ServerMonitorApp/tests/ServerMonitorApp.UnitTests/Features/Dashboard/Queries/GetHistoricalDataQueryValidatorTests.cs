using FluentValidation.Results;
using ServerMonitorApp.Application.Features.Dashboard.Queries.GetHistoricalData;

namespace ServerMonitorApp.UnitTests.Features.Dashboard.Queries
{
    public class GetHistoricalDataQueryValidatorTests
    {
        private readonly GetHistoricalDataQueryValidator _validator;

        public GetHistoricalDataQueryValidatorTests()
        {
            _validator = new GetHistoricalDataQueryValidator();
        }

        [Fact]
        public async Task Validate_ValidQuery_ReturnsTrue()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-7),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 50
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.True(result.IsValid);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public async Task Validate_EmptyDeviceId_ReturnsFalseAndError()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.Empty,
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 50
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Mã thiết bị không được để trống.");
        }

        [Fact]
        public async Task Validate_DefaultStartTime_ReturnsFalseAndError()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = default,
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 50
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Thời gian bắt đầu không hợp lệ.");
        }

        [Fact]
        public async Task Validate_StartTimeGreaterThanEndTime_ReturnsFalseAndError()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddDays(-1), 
                PageNumber = 1,
                PageSize = 50
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");
        }

        [Fact]
        public async Task Validate_DateRangeExceeds30Days_ReturnsFalseAndError()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-31),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 50
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Khoảng thời gian truy xuất không được vượt quá 30 ngày.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validate_InvalidPageNumber_ReturnsFalseAndError(int invalidPageNumber)
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow,
                PageNumber = invalidPageNumber,
                PageSize = 50
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Số trang phải lớn hơn hoặc bằng 1.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task Validate_PageSizeTooSmall_ReturnsFalseAndError(int invalidPageSize)
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = invalidPageSize
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Kích thước trang phải lớn hơn hoặc bằng 1.");
        }

        [Fact]
        public async Task Validate_PageSizeExceedsMaximum_ReturnsFalseAndError()
        {
            GetHistoricalDataQuery query = new GetHistoricalDataQuery
            {
                DeviceId = Guid.NewGuid(),
                StartTime = DateTime.UtcNow.AddDays(-1),
                EndTime = DateTime.UtcNow,
                PageNumber = 1,
                PageSize = 1001
            };

            ValidationResult result = await _validator.ValidateAsync(query);

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.ErrorMessage == "Không được lấy quá 1000 bản ghi mỗi trang.");
        }
    }
}