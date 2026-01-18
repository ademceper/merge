using FluentValidation;

namespace Merge.Application.Marketing.Commands.SendTestEmail;

public class SendTestEmailCommandValidator : AbstractValidator<SendTestEmailCommand>
{
    public SendTestEmailCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");

        RuleFor(x => x.TestEmail)
            .NotEmpty().WithMessage("Test e-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(256).WithMessage("E-posta adresi en fazla 256 karakter olabilir.");
    }
}
