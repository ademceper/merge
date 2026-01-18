using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Support;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.CloseTicket;

public class CloseTicketCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, ILogger<CloseTicketCommandHandler> logger) : IRequestHandler<CloseTicketCommand, bool>
{

    public async Task<bool> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            logger.LogWarning("Ticket {TicketId} not found for closure", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        ticket.Close();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Ticket {TicketNumber} closed successfully", ticket.TicketNumber);

        // Send closure email
        try
        {
            await emailService.SendEmailAsync(
                ticket.User?.Email ?? string.Empty,
                $"Ticket Closed - {ticket.TicketNumber}",
                $"Your support ticket #{ticket.TicketNumber} has been closed. If you need further assistance, please open a new ticket.",
                true,
                cancellationToken);
            logger.LogInformation("Closure email sent for ticket {TicketNumber}", ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send closure email for ticket {TicketNumber}", ticket.TicketNumber);
            // Exception'ı yutmayız, sadece loglarız - ticket kapatıldı, email gönderilemedi
        }

        return true;
    }
}
