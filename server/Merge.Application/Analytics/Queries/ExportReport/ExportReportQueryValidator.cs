using FluentValidation;

namespace Merge.Application.Analytics.Queries.ExportReport;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class ExportReportQueryValidator : AbstractValidator<ExportReportQuery>
{
    public ExportReportQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rapor ID zorunludur");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

