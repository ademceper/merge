using FluentValidation;
using Merge.Application.Configuration;
using Microsoft.Extensions.Options;

namespace Merge.Application.Support.Commands.AddMessage;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class AddMessageCommandValidator(IOptions<SupportSettings> settings) : AbstractValidator<AddMessageCommand>
{
    private readonly SupportSettings config = settings.Value;

    public AddMessageCommandValidator() : this(Options.Create(new SupportSettings()))
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID boş olamaz");

        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Mesaj boş olamaz")
            .MinimumLength(config.MinMessageContentLength).WithMessage($"Mesaj en az {config.MinMessageContentLength} karakter olmalıdır")
            .MaximumLength(config.MaxTicketMessageLength)
            .WithMessage($"Mesaj en fazla {config.MaxTicketMessageLength} karakter olmalıdır");
    }
}
