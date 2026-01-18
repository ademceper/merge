using FluentValidation;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetBlogPostBySlug;

public class GetBlogPostBySlugQueryValidator : AbstractValidator<GetBlogPostBySlugQuery>
{
    public GetBlogPostBySlugQueryValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithMessage("Blog post slug'ı zorunludur.");
    }
}

