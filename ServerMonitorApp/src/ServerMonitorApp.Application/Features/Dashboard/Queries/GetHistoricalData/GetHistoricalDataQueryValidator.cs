using FluentValidation;

namespace ServerMonitorApp.Application.Features.Dashboard.Queries.GetHistoricalData
{
    public class GetHistoricalDataQueryValidator : AbstractValidator<GetHistoricalDataQuery>
    {
        public GetHistoricalDataQueryValidator()
        {
            RuleFor(x => x.DeviceId).NotEmpty().WithMessage("Mã thiết bị không được để trống.");
            RuleFor(x => x.StartTime).NotEmpty().WithMessage("Thời gian bắt đầu không hợp lệ.");
            RuleFor(x => x.EndTime).NotEmpty().WithMessage("Thời gian kết thúc không hợp lệ.");

            RuleFor(x => x)
                .Must(x => x.StartTime <= x.EndTime).WithMessage("Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.")
                .Must(x => (x.EndTime - x.StartTime).TotalDays <= 30).WithMessage("Khoảng thời gian truy xuất không được vượt quá 30 ngày.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1).WithMessage("Số trang phải lớn hơn hoặc bằng 1.");

            RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1).WithMessage("Kích thước trang phải lớn hơn hoặc bằng 1.")
                .LessThanOrEqualTo(1000).WithMessage("Không được lấy quá 1000 bản ghi mỗi trang.");
        }
    }
}
