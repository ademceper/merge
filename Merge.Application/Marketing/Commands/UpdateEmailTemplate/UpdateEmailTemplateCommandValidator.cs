using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Commands.UpdateEmailTemplate;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class UpdateEmailTemplateCommandValidator : AbstractValidator<UpdateEmailTemplateCommand>
{
    public UpdateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Template ID zorunludur.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Template adı en fazla 200 karakter olabilir.")
            .MinimumLength(2).When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Template adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");

        RuleFor(x => x.Subject)
            .MaximumLength(200).WithMessage("Konu en fazla 200 karakter olabilir.")
            .MinimumLength(2).When(x => !string.IsNullOrEmpty(x.Subject))
            .WithMessage("Konu en az 2 karakter olmalıdır.");

        RuleFor(x => x.HtmlContent)
            .MinimumLength(10).When(x => !string.IsNullOrEmpty(x.HtmlContent))
            .WithMessage("HTML içerik en az 10 karakter olmalıdır.");

        RuleFor(x => x.TextContent)
            .MaximumLength(50000).WithMessage("Metin içerik en fazla 50000 karakter olabilir.");

        RuleFor(x => x.Type)
            .Must(type => string.IsNullOrEmpty(type) || Enum.TryParse<EmailTemplateType>(type, true, out _))
            .When(x => !string.IsNullOrEmpty(x.Type))
            .WithMessage("Geçerli bir template tipi seçiniz.");
    }
}
