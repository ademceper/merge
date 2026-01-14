using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.AddMessage;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class AddMessageCommandValidator : AbstractValidator<AddMessageCommand>
{
    public AddMessageCommandValidator(IOptions<SupportSettings> settings)
    {
        var supportSettings = settings.Value;

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Mesaj boş olamaz")
            .MinimumLength(supportSettings.MinMessageContentLength).WithMessage($"Mesaj en az {supportSettings.MinMessageContentLength} karakter olmalıdır")
            .MaximumLength(supportSettings.MaxTicketMessageLength)
            .WithMessage($"Mesaj en fazla {supportSettings.MaxTicketMessageLength} karakter olmalıdır");
    }
}
