using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetCMSPageBySlug;

public class GetCMSPageBySlugQueryValidator : AbstractValidator<GetCMSPageBySlugQuery>
{
    public GetCMSPageBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Slug zorunludur.")
            .MaximumLength(200)
            .WithMessage("Slug en fazla 200 karakter olabilir.");
    }
}

