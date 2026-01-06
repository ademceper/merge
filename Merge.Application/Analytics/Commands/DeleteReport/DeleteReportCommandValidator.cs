using FluentValidation;

namespace Merge.Application.Analytics.Commands.DeleteReport;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteReportCommandValidator : AbstractValidator<DeleteReportCommand>
{
    public DeleteReportCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rapor ID zorunludur");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

