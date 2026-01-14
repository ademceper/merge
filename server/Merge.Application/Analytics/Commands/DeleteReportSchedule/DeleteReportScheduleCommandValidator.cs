using FluentValidation;

namespace Merge.Application.Analytics.Commands.DeleteReportSchedule;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
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

