using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetLandingPageBySlug;

public class GetLandingPageBySlugQueryValidator : AbstractValidator<GetLandingPageBySlugQuery>
{
    public GetLandingPageBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug gereklidir")
            .MaximumLength(200).WithMessage("Slug en fazla 200 karakter olabilir");
    }
}

