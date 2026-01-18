using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.SendLiveChatMessage;

public class SendLiveChatMessageCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<SendLiveChatMessageCommand>
{
    private readonly SupportSettings config = settings.Value;

    public SendLiveChatMessageCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID boş olamaz");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Mesaj içeriği boş olamaz")
            .MinimumLength(config.MinMessageContentLength).WithMessage($"Mesaj içeriği en az {config.MinMessageContentLength} karakter olmalıdır")
            .MaximumLength(config.MaxLiveChatMessageLength)
            .WithMessage($"Mesaj içeriği en fazla {config.MaxLiveChatMessageLength} karakter olmalıdır");

        RuleFor(x => x.MessageType)
            .MaximumLength(config.MaxMessageTypeLength).WithMessage($"Mesaj tipi en fazla {config.MaxMessageTypeLength} karakter olmalıdır");

        When(x => !string.IsNullOrEmpty(x.FileUrl), () =>
        {
            RuleFor(x => x.FileUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL giriniz");
        });

        When(x => !string.IsNullOrEmpty(x.FileName), () =>
        {
            RuleFor(x => x.FileName)
                .MaximumLength(config.MaxAttachmentFileNameLength)
                .WithMessage($"Dosya adı en fazla {config.MaxAttachmentFileNameLength} karakter olmalıdır");
        });
    }
}
