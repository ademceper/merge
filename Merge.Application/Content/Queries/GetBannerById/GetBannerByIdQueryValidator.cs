using FluentValidation;

namespace Merge.Application.Content.Queries.GetBannerById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetBannerByIdQueryValidator : AbstractValidator<GetBannerByIdQuery>
{
    public GetBannerByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Banner ID'si zorunludur.");
    }
}

