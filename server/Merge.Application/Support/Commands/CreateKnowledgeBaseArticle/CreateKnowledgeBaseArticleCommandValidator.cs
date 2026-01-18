using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateKnowledgeBaseArticle;

public class CreateKnowledgeBaseArticleCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<CreateKnowledgeBaseArticleCommand>
{
    private readonly SupportSettings config = settings.Value;

    public CreateKnowledgeBaseArticleCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Yazar ID boş olamaz");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz")
            .MinimumLength(config.MinArticleTitleLength).WithMessage($"Başlık en az {config.MinArticleTitleLength} karakter olmalıdır")
            .MaximumLength(config.MaxArticleTitleLength)
            .WithMessage($"Başlık en fazla {config.MaxArticleTitleLength} karakter olmalıdır");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz")
            .MinimumLength(config.MinArticleContentLength).WithMessage($"İçerik en az {config.MinArticleContentLength} karakter olmalıdır")
            .MaximumLength(config.MaxArticleContentLength)
            .WithMessage($"İçerik en fazla {config.MaxArticleContentLength} karakter olmalıdır");

        When(x => !string.IsNullOrEmpty(x.Excerpt), () =>
        {
            RuleFor(x => x.Excerpt)
                .MaximumLength(config.MaxArticleExcerptLength)
                .WithMessage($"Özet en fazla {config.MaxArticleExcerptLength} karakter olmalıdır");
        });

        RuleFor(x => x.Status)
            .Must(s => s == "Draft" || s == "Published" || s == "Archived")
            .WithMessage("Durum geçerli değil");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(config.MinDisplayOrder).WithMessage($"Görüntüleme sırası {config.MinDisplayOrder} veya daha büyük olmalıdır");
    }
}
