using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.MarkMessagesAsRead;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class MarkMessagesAsReadCommandHandler : IRequestHandler<MarkMessagesAsReadCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MarkMessagesAsReadCommandHandler> _logger;

    public MarkMessagesAsReadCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<MarkMessagesAsReadCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(MarkMessagesAsReadCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Marking messages as read for session {SessionId} by user {UserId}", 
            request.SessionId, request.UserId);

        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Live chat session {SessionId} not found for marking messages as read", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        var messages = await _context.Set<LiveChatMessage>()
            .Where(m => m.SessionId == request.SessionId && !m.IsRead && m.SenderId != request.UserId)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        foreach (var message in messages)
        {
            message.MarkAsRead();
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.MarkMessagesAsRead();

        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked {MessageCount} messages as read for session {SessionId}", 
            messages.Count, request.SessionId);

        return true;
    }
}
