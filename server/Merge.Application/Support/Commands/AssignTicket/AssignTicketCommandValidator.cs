using FluentValidation;

namespace Merge.Application.Support.Commands.AssignTicket;

public class AssignTicketCommandValidator : AbstractValidator<AssignTicketCommand>
{
    public AssignTicketCommandValidator()
    {
        RuleFor(x => x.TicketId)
            .NotEmpty().WithMessage("Ticket ID boş olamaz");

        RuleFor(x => x.AssignedToId)
            .NotEmpty().WithMessage("Atanacak kullanıcı ID boş olamaz");
    }
}
