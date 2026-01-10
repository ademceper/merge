using FluentValidation;

namespace Merge.Application.Marketing.Queries.GetEmailTemplateById;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class GetEmailTemplateByIdQueryValidator : AbstractValidator<GetEmailTemplateByIdQuery>
{
    public GetEmailTemplateByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID zorunludur.");
    }
}
