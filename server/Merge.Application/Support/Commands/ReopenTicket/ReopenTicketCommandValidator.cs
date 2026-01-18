using FluentValidation;

namespace Merge.Application.Support.Commands.ReopenTicket;

public class ReopenTicketCommandValidator : AbstractValidator<ReopenTicketCommand>
{
    public ReopenTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");
    }
}
