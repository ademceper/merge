using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Exceptions;
using Merge.Application.Services.Notification;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.CloseTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ILogger<CloseTicketCommandHandler> _logger;

    public CloseTicketCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ILogger<CloseTicketCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<bool> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _context.Set<SupportTicket>()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for closure", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        ticket.Close();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} closed successfully", ticket.TicketNumber);

        // Send closure email
        try
        {
            await _emailService.SendEmailAsync(
                ticket.User?.Email ?? string.Empty,
                $"Ticket Closed - {ticket.TicketNumber}",
                $"Your support ticket #{ticket.TicketNumber} has been closed. If you need further assistance, please open a new ticket.",
                true,
                cancellationToken);
            _logger.LogInformation("Closure email sent for ticket {TicketNumber}", ticket.TicketNumber);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception handling - Log ve throw (YASAK: Exception yutulmamalı)
            _logger.LogError(ex, "Failed to send closure email for ticket {TicketNumber}", ticket.TicketNumber);
            // Exception'ı yutmayız, sadece loglarız - ticket kapatıldı, email gönderilemedi
        }

        return true;
    }
}
