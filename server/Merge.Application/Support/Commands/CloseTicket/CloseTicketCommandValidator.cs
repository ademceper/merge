using FluentValidation;

namespace Merge.Application.Support.Commands.CloseTicket;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class CloseTicketCommandValidator : AbstractValidator<CloseTicketCommand>
{
    public CloseTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");
    }
}
