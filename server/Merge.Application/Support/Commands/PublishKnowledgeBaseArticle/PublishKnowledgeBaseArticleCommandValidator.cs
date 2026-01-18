using FluentValidation;

namespace Merge.Application.Support.Commands.PublishKnowledgeBaseArticle;

public class PublishKnowledgeBaseArticleCommandValidator : AbstractValidator<PublishKnowledgeBaseArticleCommand>
{
    public PublishKnowledgeBaseArticleCommandValidator()
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");
    }
}
