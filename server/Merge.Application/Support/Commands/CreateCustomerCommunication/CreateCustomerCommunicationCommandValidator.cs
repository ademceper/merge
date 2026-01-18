using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.CreateCustomerCommunication;

public class CreateCustomerCommunicationCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<CreateCustomerCommunicationCommand>
{
    private readonly SupportSettings config = settings.Value;

    public CreateCustomerCommunicationCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.CommunicationType)
            .NotEmpty().WithMessage("İletişim tipi boş olamaz")
            .MaximumLength(config.MaxCommunicationTypeLength).WithMessage($"İletişim tipi en fazla {config.MaxCommunicationTypeLength} karakter olmalıdır");

        RuleFor(x => x.Channel)
            .NotEmpty().WithMessage("Kanal boş olamaz")
            .MaximumLength(config.MaxChannelLength).WithMessage($"Kanal en fazla {config.MaxChannelLength} karakter olmalıdır");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Konu boş olamaz")
            .MinimumLength(config.MinCategoryNameLength).WithMessage($"Konu en az {config.MinCategoryNameLength} karakter olmalıdır")
            .MaximumLength(config.MaxCommunicationSubjectLength)
            .WithMessage($"Konu en fazla {config.MaxCommunicationSubjectLength} karakter olmalıdır");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz")
            .MinimumLength(config.MinMessageContentLength).WithMessage($"İçerik en az {config.MinMessageContentLength} karakter olmalıdır")
            .MaximumLength(config.MaxCommunicationContentLength)
            .WithMessage($"İçerik en fazla {config.MaxCommunicationContentLength} karakter olmalıdır");

        RuleFor(x => x.Direction)
            .Must(d => d == "Inbound" || d == "Outbound")
            .WithMessage("Yön geçerli değil");

        When(x => !string.IsNullOrEmpty(x.RecipientEmail), () =>
        {
            RuleFor(x => x.RecipientEmail)
                .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz")
                .MaximumLength(config.MaxEmailLength).WithMessage($"E-posta adresi en fazla {config.MaxEmailLength} karakter olmalıdır");
        });

        When(x => !string.IsNullOrEmpty(x.RecipientPhone), () =>
        {
            RuleFor(x => x.RecipientPhone)
                .MaximumLength(config.MaxPhoneNumberLength).WithMessage($"Telefon numarası en fazla {config.MaxPhoneNumberLength} karakter olmalıdır");
        });
    }
}
