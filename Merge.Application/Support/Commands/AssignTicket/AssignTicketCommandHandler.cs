using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.AssignTicket;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AssignTicketCommandHandler : IRequestHandler<AssignTicketCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignTicketCommandHandler> _logger;

    public AssignTicketCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AssignTicketCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(AssignTicketCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Assigning ticket {TicketId} to user {AssignedToId}", request.TicketId, request.AssignedToId);

        var ticket = await _context.Set<SupportTicket>()
            .FirstOrDefaultAsync(t => t.Id == request.TicketId, cancellationToken);

        if (ticket == null)
        {
            _logger.LogWarning("Ticket {TicketId} not found for assignment", request.TicketId);
            throw new NotFoundException("Destek bileti", request.TicketId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        ticket.AssignTo(request.AssignedToId);

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Ticket {TicketNumber} assigned to user {AssignedToId}", ticket.TicketNumber, request.AssignedToId);

        return true;
    }
}
