using FluentValidation;

namespace Merge.Application.Content.Queries.GetBlogPostById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetBlogPostByIdQueryValidator : AbstractValidator<GetBlogPostByIdQuery>
{
    public GetBlogPostByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Blog post ID'si zorunludur.");
    }
}

