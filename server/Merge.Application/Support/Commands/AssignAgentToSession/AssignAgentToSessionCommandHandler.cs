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

namespace Merge.Application.Support.Commands.AssignAgentToSession;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AssignAgentToSessionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<AssignAgentToSessionCommandHandler> logger) : IRequestHandler<AssignAgentToSessionCommand, bool>
{

    public async Task<bool> Handle(AssignAgentToSessionCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Assigning agent {AgentId} to live chat session {SessionId}",
            request.AgentId, request.SessionId);

        var session = await context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            logger.LogWarning("Live chat session {SessionId} not found for agent assignment", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        session.AssignAgent(request.AgentId);
        
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Agent {AgentId} assigned to live chat session {SessionId} successfully",
            request.AgentId, request.SessionId);

        return true;
    }
}
