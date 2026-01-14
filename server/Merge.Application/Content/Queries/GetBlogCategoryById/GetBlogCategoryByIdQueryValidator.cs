using FluentValidation;

namespace Merge.Application.Content.Queries.GetBlogCategoryById;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class GetBlogCategoryByIdQueryValidator : AbstractValidator<GetBlogCategoryByIdQuery>
{
    public GetBlogCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

