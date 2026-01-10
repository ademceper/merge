using FluentValidation;

namespace Merge.Application.Marketing.Commands.UpdateEmailCampaign;

public class UpdateEmailCampaignCommandValidator : AbstractValidator<UpdateEmailCampaignCommand>
{
    public UpdateEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Kampanya ID'si zorunludur.");

        When(x => !string.IsNullOrEmpty(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Kampanya adı en fazla 200 karakter olabilir.")
                .MinimumLength(2).WithMessage("Kampanya adı en az 2 karakter olmalıdır.");
        });

        When(x => !string.IsNullOrEmpty(x.Subject), () =>
        {
            RuleFor(x => x.Subject)
                .MaximumLength(200).WithMessage("Konu en fazla 200 karakter olabilir.")
                .MinimumLength(2).WithMessage("Konu en az 2 karakter olmalıdır.");
        });

        When(x => !string.IsNullOrEmpty(x.FromEmail), () =>
        {
            RuleFor(x => x.FromEmail)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");
        });

        When(x => !string.IsNullOrEmpty(x.ReplyToEmail), () =>
        {
            RuleFor(x => x.ReplyToEmail)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
                .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");
        });

        When(x => !string.IsNullOrEmpty(x.Content), () =>
        {
            RuleFor(x => x.Content)
                .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir.")
                .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır.");
        });
    }
}
