using FluentValidation;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseCategory;

public class DeleteKnowledgeBaseCategoryCommandValidator : AbstractValidator<DeleteKnowledgeBaseCategoryCommand>
{
    public DeleteKnowledgeBaseCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Kategori ID boş olamaz");
    }
}
