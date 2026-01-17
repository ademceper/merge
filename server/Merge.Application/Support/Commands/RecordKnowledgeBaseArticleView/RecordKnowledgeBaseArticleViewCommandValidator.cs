using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class RecordKnowledgeBaseArticleViewCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<RecordKnowledgeBaseArticleViewCommand>
{
    private readonly SupportSettings config = settings.Value;

    public RecordKnowledgeBaseArticleViewCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");

        When(x => !string.IsNullOrEmpty(x.IpAddress), () =>
        {
            RuleFor(x => x.IpAddress)
                .MaximumLength(config.MaxIpAddressLength).WithMessage($"IP adresi en fazla {config.MaxIpAddressLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.UserAgent), () =>
        {
            RuleFor(x => x.UserAgent)
                .MaximumLength(config.MaxUserAgentLength).WithMessage($"User agent en fazla {config.MaxUserAgentLength} karakter olmalıdır");
        });
    }
}
