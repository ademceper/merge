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

namespace Merge.Application.Support.Commands.ReopenTicket;

public class ReopenTicketCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ReopenTicketCommandHandler> logger) : IRequestHandler<ReopenTicketCommand, bool>
{

    public async Task<bool> Handle(ReopenTicketCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Reopening ticket {TicketId}", request.TicketId);

        var ticket = await context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            logger.LogWarning("Ticket {TicketId} not found for reopening", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        ticket.Reopen();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ticket {TicketNumber} reopened", ticket.TicketNumber);

        return true;
    }
}
