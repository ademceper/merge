using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.CloseLiveChatSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class CloseLiveChatSessionCommandHandler : IRequestHandler<CloseLiveChatSessionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CloseLiveChatSessionCommandHandler> _logger;

    public CloseLiveChatSessionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CloseLiveChatSessionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CloseLiveChatSessionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Closing live chat session {SessionId}", request.SessionId);

        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Live chat session {SessionId} not found for closure", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.Close();
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Live chat session {SessionId} closed successfully", request.SessionId);

        return true;
    }
}
