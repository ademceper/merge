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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ReopenTicketCommandHandler : IRequestHandler<ReopenTicketCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReopenTicketCommandHandler> _logger;

    public ReopenTicketCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ReopenTicketCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ReopenTicketCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Reopening ticket {TicketId}", request.TicketId);

        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for reopening", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        ticket.Reopen();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} reopened", ticket.TicketNumber);

        return true;
    }
}
