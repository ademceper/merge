using FluentValidation;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseArticle;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class DeleteKnowledgeBaseArticleCommandValidator : AbstractValidator<DeleteKnowledgeBaseArticleCommand>
{
    public DeleteKnowledgeBaseArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");
    }
}
