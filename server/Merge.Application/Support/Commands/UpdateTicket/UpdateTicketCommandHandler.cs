using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Microsoft.Extensions.Options;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.UpdateTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UpdateTicketCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<UpdateTicketCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<UpdateTicketCommand, bool>
{

    public async Task<bool> Handle(UpdateTicketCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Updating ticket {TicketId}. Subject: {Subject}, Status: {Status}, Priority: {Priority}",
            request.TicketId, request.Subject, request.Status, request.Priority);

        var ticket = await context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            logger.LogWarning("Ticket {TicketId} not found for update", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        var oldStatus = ticket.Status;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        if (!string.IsNullOrEmpty(request.Subject))
        {
            ticket.UpdateSubject(request.Subject);
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            ticket.UpdateDescription(request.Description);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            ticket.UpdateCategory(Enum.Parse<TicketCategory>(request.Category, true));
        }

        if (!string.IsNullOrEmpty(request.Priority))
        {
            ticket.UpdatePriority(Enum.Parse<TicketPriority>(request.Priority, true));
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            var newStatus = Enum.Parse<TicketStatus>(request.Status, true);
            
            // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı (semantic methods)
            if (newStatus == TicketStatus.Resolved)
            {
                ticket.Resolve();
                logger.LogInformation("Ticket {TicketNumber} marked as resolved", ticket.TicketNumber);
            }
            else if (newStatus == TicketStatus.Closed)
            {
                ticket.Close();
                logger.LogInformation("Ticket {TicketNumber} marked as closed", ticket.TicketNumber);
            }
            else
            {
                ticket.UpdateStatus(newStatus);
            }
        }

        if (request.AssignedToId.HasValue)
        {
            ticket.AssignTo(request.AssignedToId.Value);
        }

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ticket {TicketNumber} updated. Status changed from {OldStatus} to {NewStatus}",
            ticket.TicketNumber, oldStatus, ticket.Status);

        return true;
    }
}
