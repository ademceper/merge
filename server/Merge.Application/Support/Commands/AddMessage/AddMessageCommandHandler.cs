using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;
using Microsoft.Extensions.Options;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Support;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Commands.AddMessage;

public class AddMessageCommandHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, IEmailService emailService, ILogger<AddMessageCommandHandler> logger, IOptions<SupportSettings> settings) : IRequestHandler<AddMessageCommand, TicketMessageDto>
{

    public async Task<TicketMessageDto> Handle(AddMessageCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Adding message to ticket {TicketId} from user {UserId}. IsStaffResponse: {IsStaffResponse}",
            request.TicketId, request.UserId, request.IsStaffResponse);

        var ticket = await context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            logger.LogWarning("Ticket {TicketId} not found while adding message", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        var oldStatus = ticket.Status;

        var message = TicketMessage.Create(
            request.TicketId,
            ticket.TicketNumber,
            request.UserId,
            request.Message,
            request.IsStaffResponse,
            request.IsInternal);

        await context.Set<TicketMessage>().AddAsync(message, cancellationToken);

        ticket.AddMessage();
        ticket.UpdateStatusOnMessage(request.IsStaffResponse);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Message added to ticket {TicketNumber}. Response count: {ResponseCount}, Status: {OldStatus} -> {NewStatus}",
            ticket.TicketNumber, ticket.ResponseCount, oldStatus, ticket.Status);

        // Send email notification
        if (request.IsStaffResponse && !request.IsInternal)
        {
            try
            {
                await emailService.SendEmailAsync(
                    ticket.User?.Email ?? string.Empty,
                    $"New Response on Ticket {ticket.TicketNumber}",
                    $"You have received a new response on your support ticket #{ticket.TicketNumber}.",
                    true,
                    cancellationToken);
                logger.LogInformation("Response notification email sent for ticket {TicketNumber}", ticket.TicketNumber);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send response notification for ticket {TicketNumber}", ticket.TicketNumber);
                // Exception'ı yutmayız, sadece loglarız - mesaj eklendi, email gönderilemedi
            }
        }

        message = await context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Attachments)
            .FirstOrDefaultAsync(m => m.Id == message.Id, cancellationToken);

        return mapper.Map<TicketMessageDto>(message!);
    }
}
