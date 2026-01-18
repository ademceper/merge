using FluentValidation;
using Merge.Domain.Enums;

namespace Merge.Application.Marketing.Commands.CreateEmailAutomation;

public class CreateEmailAutomationCommandValidator : AbstractValidator<CreateEmailAutomationCommand>
{
    public CreateEmailAutomationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Otomasyon adı zorunludur.")
            .MaximumLength(200).WithMessage("Otomasyon adı en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Otomasyon adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Otomasyon tipi zorunludur.")
            .Must(type => Enum.TryParse<EmailAutomationType>(type, true, out _))
            .WithMessage("Geçerli bir otomasyon tipi seçiniz.");

        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("Template ID zorunludur.");

        RuleFor(x => x.DelayHours)
            .GreaterThanOrEqualTo(0).WithMessage("Gecikme saati 0 veya daha büyük olmalıdır.");
    }
}
