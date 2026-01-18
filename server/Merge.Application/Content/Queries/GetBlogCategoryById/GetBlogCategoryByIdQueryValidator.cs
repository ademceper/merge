using FluentValidation;

namespace Merge.Application.Content.Queries.GetBlogCategoryById;

public class GetBlogCategoryByIdQueryValidator : AbstractValidator<GetBlogCategoryByIdQuery>
{
    public GetBlogCategoryByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

