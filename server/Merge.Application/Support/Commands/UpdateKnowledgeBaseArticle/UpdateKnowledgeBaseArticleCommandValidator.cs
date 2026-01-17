using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseArticle;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class UpdateKnowledgeBaseArticleCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<UpdateKnowledgeBaseArticleCommand>
{
    private readonly SupportSettings config = settings.Value;

    public UpdateKnowledgeBaseArticleCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");

        When(x => !string.IsNullOrEmpty(x.Title), () =>
        {
            RuleFor(x => x.Title)
                .MinimumLength(config.MinArticleTitleLength).WithMessage($"Başlık en az {config.MinArticleTitleLength} karakter olmalıdır")
                .MaximumLength(config.MaxArticleTitleLength)
                .WithMessage($"Başlık en fazla {config.MaxArticleTitleLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Content), () =>
        {
            RuleFor(x => x.Content)
                .MinimumLength(config.MinArticleContentLength).WithMessage($"İçerik en az {config.MinArticleContentLength} karakter olmalıdır")
                .MaximumLength(config.MaxArticleContentLength)
                .WithMessage($"İçerik en fazla {config.MaxArticleContentLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Excerpt), () =>
        {
            RuleFor(x => x.Excerpt)
                .MaximumLength(config.MaxArticleExcerptLength)
                .WithMessage($"Özet en fazla {config.MaxArticleExcerptLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Status), () =>
        {
            RuleFor(x => x.Status)
                .Must(s => s == "Draft" || s == "Published" || s == "Archived")
                .WithMessage("Durum Draft, Published veya Archived olmalıdır");
        });

        When(x => x.DisplayOrder.HasValue, () =>
        {
            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(config.MinDisplayOrder).WithMessage($"Görüntüleme sırası {config.MinDisplayOrder} veya daha büyük olmalıdır");
        });
    }
}
