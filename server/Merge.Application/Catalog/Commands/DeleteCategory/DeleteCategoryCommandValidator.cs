using FluentValidation;

namespace Merge.Application.Catalog.Commands.DeleteCategory;

// ✅ BOLUM 2.1: FluentValidation (ZORUNLU)
public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Kategori ID'si zorunludur.");
    }
}
