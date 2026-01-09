using FluentValidation;

namespace Merge.Application.Content.Queries.GetLandingPageById;

// ✅ BOLUM 2.3: FluentValidation (ZORUNLU)
public class GetLandingPageByIdQueryValidator : AbstractValidator<GetLandingPageByIdQuery>
{
    public GetLandingPageByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID gereklidir");
    }
}

