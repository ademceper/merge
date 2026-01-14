using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.UpdateKnowledgeBaseArticle;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class UpdateKnowledgeBaseArticleCommandValidator : AbstractValidator<UpdateKnowledgeBaseArticleCommand>
{
    public UpdateKnowledgeBaseArticleCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");

        When(x => !string.IsNullOrEmpty(x.Title), () =>
        {
            RuleFor(x => x.Title)
                .MinimumLength(supportSettings.MinArticleTitleLength).WithMessage($"Başlık en az {supportSettings.MinArticleTitleLength} karakter olmalıdır")
                .MaximumLength(supportSettings.MaxArticleTitleLength)
                .WithMessage($"Başlık en fazla {supportSettings.MaxArticleTitleLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Content), () =>
        {
            RuleFor(x => x.Content)
                .MinimumLength(supportSettings.MinArticleContentLength).WithMessage($"İçerik en az {supportSettings.MinArticleContentLength} karakter olmalıdır")
                .MaximumLength(supportSettings.MaxArticleContentLength)
                .WithMessage($"İçerik en fazla {supportSettings.MaxArticleContentLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.Excerpt), () =>
        {
            RuleFor(x => x.Excerpt)
                .MaximumLength(supportSettings.MaxArticleExcerptLength)
                .WithMessage($"Özet en fazla {supportSettings.MaxArticleExcerptLength} karakter olmalıdır");
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
                .GreaterThanOrEqualTo(supportSettings.MinDisplayOrder).WithMessage($"Görüntüleme sırası {supportSettings.MinDisplayOrder} veya daha büyük olmalıdır");
        });
    }
}
