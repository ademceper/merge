using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateCustomerCommunication;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CreateCustomerCommunicationCommandValidator : AbstractValidator<CreateCustomerCommunicationCommand>
{
    public CreateCustomerCommunicationCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.CommunicationType)
            .NotEmpty().WithMessage("İletişim tipi boş olamaz")
            .MaximumLength(supportSettings.MaxCommunicationTypeLength).WithMessage($"İletişim tipi en fazla {supportSettings.MaxCommunicationTypeLength} karakter olmalıdır");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Kanal boş olamaz")
            .MaximumLength(supportSettings.MaxChannelLength).WithMessage($"Kanal en fazla {supportSettings.MaxChannelLength} karakter olmalıdır");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu boş olamaz")
            .MinimumLength(supportSettings.MinCategoryNameLength).WithMessage($"Konu en az {supportSettings.MinCategoryNameLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxCommunicationSubjectLength)
            .WithMessage($"Konu en fazla {supportSettings.MaxCommunicationSubjectLength} karakter olmalıdır");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz")
            .MinimumLength(supportSettings.MinMessageContentLength).WithMessage($"İçerik en az {supportSettings.MinMessageContentLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxCommunicationContentLength)
            .WithMessage($"İçerik en fazla {supportSettings.MaxCommunicationContentLength} karakter olmalıdır");

        RuleFor(x => x.Direction)
            .Must(d => d == "Inbound" || d == "Outbound")
            .WithMessage("Yön geçerli değil");

        When(x => !string.IsNullOrEmpty(x.RecipientEmail), () =>
        {
            RuleFor(x => x.RecipientEmail)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
                .MaximumLength(supportSettings.MaxEmailLength).WithMessage($"E-posta adresi en fazla {supportSettings.MaxEmailLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.RecipientPhone), () =>
        {
            RuleFor(x => x.RecipientPhone)
                .MaximumLength(supportSettings.MaxPhoneNumberLength).WithMessage($"Telefon numarası en fazla {supportSettings.MaxPhoneNumberLength} karakter olmalıdır");
        });
    }
}
