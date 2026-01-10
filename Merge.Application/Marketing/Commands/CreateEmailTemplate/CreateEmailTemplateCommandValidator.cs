using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Commands.CreateEmailTemplate;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateEmailTemplateCommandValidator : AbstractValidator<CreateEmailTemplateCommand>
{
    public CreateEmailTemplateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Template adı zorunludur.")
            .MaximumLength(200).WithMessage("Template adı en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Template adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu zorunludur.")
            .MaximumLength(200).WithMessage("Konu en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Konu en az 2 karakter olmalıdır.");

        RuleFor(x => x.HtmlContent)
            .NotEmpty().WithMessage("HTML içerik zorunludur.")
            .MinimumLength(10).WithMessage("HTML içerik en az 10 karakter olmalıdır.");

        RuleFor(x => x.TextContent)
            .MaximumLength(50000).WithMessage("Metin içerik en fazla 50000 karakter olabilir.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Template tipi zorunludur.")
            .Must(type => Enum.TryParse<EmailTemplateType>(type, true, out _))
            .WithMessage("Geçerli bir template tipi seçiniz.");
    }
}
