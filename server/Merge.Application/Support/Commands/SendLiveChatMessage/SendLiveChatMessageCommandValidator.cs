using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.SendLiveChatMessage;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class SendLiveChatMessageCommandValidator : AbstractValidator<SendLiveChatMessageCommand>
{
    public SendLiveChatMessageCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID boş olamaz");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Mesaj içeriği boş olamaz")
            .MinimumLength(supportSettings.MinMessageContentLength).WithMessage($"Mesaj içeriği en az {supportSettings.MinMessageContentLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxLiveChatMessageLength)
            .WithMessage($"Mesaj içeriği en fazla {supportSettings.MaxLiveChatMessageLength} karakter olmalıdır");

        RuleFor(x => x.MessageType)
            .MaximumLength(supportSettings.MaxMessageTypeLength).WithMessage($"Mesaj tipi en fazla {supportSettings.MaxMessageTypeLength} karakter olmalıdır");

        When(x => !string.IsNullOrEmpty(x.FileUrl), () =>
        {
            RuleFor(x => x.FileUrl)
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Geçerli bir URL giriniz");
        });

        When(x => !string.IsNullOrEmpty(x.FileName), () =>
        {
            RuleFor(x => x.FileName)
                .MaximumLength(supportSettings.MaxAttachmentFileNameLength)
                .WithMessage($"Dosya adı en fazla {supportSettings.MaxAttachmentFileNameLength} karakter olmalıdır");
        });
    }
}
