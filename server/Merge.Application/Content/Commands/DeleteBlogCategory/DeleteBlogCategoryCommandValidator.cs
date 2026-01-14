using FluentValidation;

namespace Merge.Application.Content.Commands.DeleteBlogCategory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteBlogCategoryCommandValidator : AbstractValidator<DeleteBlogCategoryCommand>
{
    public DeleteBlogCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}

