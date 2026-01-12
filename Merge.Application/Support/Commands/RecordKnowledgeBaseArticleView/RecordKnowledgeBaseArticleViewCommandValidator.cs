using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;
using Merge.Domain.Modules.Identity;

namespace Merge.Application.Support.Commands.RecordKnowledgeBaseArticleView;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class RecordKnowledgeBaseArticleViewCommandValidator : AbstractValidator<RecordKnowledgeBaseArticleViewCommand>
{
    public RecordKnowledgeBaseArticleViewCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.ArticleId)
            .NotEmpty().WithMessage("Makale ID boş olamaz");

        When(x => !string.IsNullOrEmpty(x.IpAddress), () =>
        {
            RuleFor(x => x.IpAddress)
                .MaximumLength(supportSettings.MaxIpAddressLength).WithMessage($"IP adresi en fazla {supportSettings.MaxIpAddressLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.UserAgent), () =>
        {
            RuleFor(x => x.UserAgent)
                .MaximumLength(supportSettings.MaxUserAgentLength).WithMessage($"User agent en fazla {supportSettings.MaxUserAgentLength} karakter olmalıdır");
        });
    }
}
