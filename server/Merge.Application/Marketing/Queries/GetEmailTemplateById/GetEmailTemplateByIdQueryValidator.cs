using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailTemplateById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetEmailTemplateByIdQueryValidator : AbstractValidator<GetEmailTemplateByIdQuery>
{
    public GetEmailTemplateByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID zorunludur.");
    }
}
