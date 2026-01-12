using FluentValidation;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class DeleteKnowledgeBaseCategoryCommandValidator : AbstractValidator<DeleteKnowledgeBaseCategoryCommand>
{
    public DeleteKnowledgeBaseCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz");
    }
}
