using FluentValidation;

namespace Merge.Application.Support.Commands.ReopenTicket;

// ✅ BOLUM 2.1: Pipeline Behaviors - ValidationBehavior (ZORUNLU)
public class ReopenTicketCommandValidator : AbstractValidator<ReopenTicketCommand>
{
    public ReopenTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");
    }
}
