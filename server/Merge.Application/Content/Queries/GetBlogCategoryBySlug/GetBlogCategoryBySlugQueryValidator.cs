using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetBlogCategoryBySlug;

public class GetBlogCategoryBySlugQueryValidator : AbstractValidator<GetBlogCategoryBySlugQuery>
{
    public GetBlogCategoryBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Kategori slug'ı zorunludur.");
    }
}

