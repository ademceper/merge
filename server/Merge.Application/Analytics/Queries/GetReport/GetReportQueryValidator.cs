using FluentValidation;

namespace Merge.Application.Analytics.Queries.GetReport;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetReportQueryValidator : AbstractValidator<GetReportQuery>
{
    public GetReportQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rapor ID zorunludur");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID zorunludur");
    }
}

