using FluentValidation;

namespace Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class PublishKnowledgeBaseArticleCommandValidator : AbstractValidator<PublishKnowledgeBaseArticleCommand>
{
    public PublishKnowledgeBaseArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");
    }
}
