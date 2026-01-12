using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;

namespace Merge.Application.Support.Commands.AssignAgentToSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AssignAgentToSessionCommandHandler : IRequestHandler<AssignAgentToSessionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AssignAgentToSessionCommandHandler> _logger;

    public AssignAgentToSessionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AssignAgentToSessionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(AssignAgentToSessionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Assigning agent {AgentId} to live chat session {SessionId}",
            request.AgentId, request.SessionId);

        var session = await _context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            _logger.LogWarning("Live chat session {SessionId} not found for agent assignment", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.AssignAgent(request.AgentId);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Agent {AgentId} assigned to live chat session {SessionId} successfully",
            request.AgentId, request.SessionId);

        return true;
    }
}
