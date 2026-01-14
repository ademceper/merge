using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseArticle;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateKnowledgeBaseArticleCommandValidator : AbstractValidator<CreateKnowledgeBaseArticleCommand>
{
    public CreateKnowledgeBaseArticleCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Yazar ID boş olamaz");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz")
            .MinimumLength(supportSettings.MinArticleTitleLength).WithMessage($"Başlık en az {supportSettings.MinArticleTitleLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxArticleTitleLength)
            .WithMessage($"Başlık en fazla {supportSettings.MaxArticleTitleLength} karakter olmalıdır");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz")
            .MinimumLength(supportSettings.MinArticleContentLength).WithMessage($"İçerik en az {supportSettings.MinArticleContentLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxArticleContentLength)
            .WithMessage($"İçerik en fazla {supportSettings.MaxArticleContentLength} karakter olmalıdır");

        When(x => !string.IsNullOrEmpty(x.Excerpt), () =>
        {
            RuleFor(x => x.Excerpt)
                .MaximumLength(supportSettings.MaxArticleExcerptLength)
                .WithMessage($"Özet en fazla {supportSettings.MaxArticleExcerptLength} karakter olmalıdır");
        });

        RuleFor(x => x.Status)
            .Must(s => s == "Draft" || s == "Published" || s == "Archived")
            .WithMessage("Durum geçerli değil");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(supportSettings.MinDisplayOrder).WithMessage($"Görüntüleme sırası {supportSettings.MinDisplayOrder} veya daha büyük olmalıdır");
    }
}
