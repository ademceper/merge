using FluentValidation;

namespace Merge.Application.Marketing.Commands.CreateEmailCampaign;

// ✅ BOLUM 2.0: FluentValidation (ZORUNLU)
public class CreateEmailCampaignCommandValidator : AbstractValidator<CreateEmailCampaignCommand>
{
    public CreateEmailCampaignCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kampanya adı zorunludur.")
            .MaximumLength(200).WithMessage("Kampanya adı en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Kampanya adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu zorunludur.")
            .MaximumLength(200).WithMessage("Konu en fazla 200 karakter olabilir.")
            .MinimumLength(2).WithMessage("Konu en az 2 karakter olmalıdır.");

        RuleFor(x => x.FromName)
            .NotEmpty().WithMessage("Gönderen adı zorunludur.")
            .MaximumLength(100).WithMessage("Gönderen adı en fazla 100 karakter olabilir.")
            .MinimumLength(2).WithMessage("Gönderen adı en az 2 karakter olmalıdır.");

        RuleFor(x => x.FromEmail)
            .NotEmpty().WithMessage("Gönderen e-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");

        RuleFor(x => x.ReplyToEmail)
            .NotEmpty().WithMessage("Yanıt e-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")
            .MaximumLength(200).WithMessage("E-posta adresi en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik zorunludur.")
            .MaximumLength(50000).WithMessage("İçerik en fazla 50000 karakter olabilir.")
            .MinimumLength(10).WithMessage("İçerik en az 10 karakter olmalıdır.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Kampanya tipi zorunludur.")
            .Must(type => Enum.TryParse<Merge.Domain.Enums.EmailCampaignType>(type, true, out _))
            .WithMessage("Geçerli bir kampanya tipi seçiniz.");

        RuleFor(x => x.TargetSegment)
            .NotEmpty().WithMessage("Hedef segment zorunludur.")
            .MaximumLength(100).WithMessage("Hedef segment en fazla 100 karakter olabilir.");
    }
}
