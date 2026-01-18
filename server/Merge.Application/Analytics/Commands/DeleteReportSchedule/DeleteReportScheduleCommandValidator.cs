using FluentValidation;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

public class DeleteReportScheduleCommandValidator : AbstractValidator<DeleteReportScheduleCommand>
{
    public DeleteReportScheduleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Zamanlama ID zorunludur");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

