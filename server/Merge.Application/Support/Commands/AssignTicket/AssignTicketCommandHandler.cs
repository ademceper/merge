using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.AssignTicket;

public class AssignTicketCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<AssignTicketCommandHandler> logger) : IRequestHandler<AssignTicketCommand, bool>
{

    public async Task<bool> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning ticket {TicketId} to user {AssignedToId}", request.TicketId, request.AssignedToId);

        var ticket = await context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            logger.LogWarning("Ticket {TicketId} not found for assignment", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        ticket.AssignTo(request.AssignedToId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ticket {TicketNumber} assigned to user {AssignedToId}", ticket.TicketNumber, request.AssignedToId);

        return true;
    }
}
