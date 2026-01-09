using FluentValidation;

namespace Merge.Application.Content.Queries.GetCMSPageById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetCMSPageByIdQueryValidator : AbstractValidator<GetCMSPageByIdQuery>
{
    public GetCMSPageByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("CMS sayfası ID'si zorunludur.");
    }
}

