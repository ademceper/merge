using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetPageBuilderBySlug;

public class GetPageBuilderBySlugQueryValidator : AbstractValidator<GetPageBuilderBySlugQuery>
{
    public GetPageBuilderBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug gereklidir")
            .MaximumLength(200).WithMessage("Slug en fazla 200 karakter olabilir");
    }
}

