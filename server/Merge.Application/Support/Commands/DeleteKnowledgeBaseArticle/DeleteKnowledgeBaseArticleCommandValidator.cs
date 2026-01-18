using FluentValidation;

namespace Merge.Application.Support.Commands.DeleteKnowledgeBaseArticle;

public class DeleteKnowledgeBaseArticleCommandValidator : AbstractValidator<DeleteKnowledgeBaseArticleCommand>
{
    public DeleteKnowledgeBaseArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");
    }
}
