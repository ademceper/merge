using FluentValidation;

namespace Merge.Application.Analytics.Commands.ToggleReportSchedule;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ToggleReportScheduleCommandValidator : AbstractValidator<ToggleReportScheduleCommand>
{
    public ToggleReportScheduleCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Zamanlama ID zorunludur");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

