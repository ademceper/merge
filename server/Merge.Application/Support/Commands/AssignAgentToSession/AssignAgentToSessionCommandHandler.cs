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

public class AssignAgentToSessionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<AssignAgentToSessionCommandHandler> logger) : IRequestHandler<AssignAgentToSessionCommand, bool>
{

    public async Task<bool> Handle(AssignAgentToSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Assigning agent {AgentId} to live chat session {SessionId}",
            request.AgentId, request.SessionId);

        var session = await context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            logger.LogWarning("Live chat session {SessionId} not found for agent assignment", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        session.AssignAgent(request.AgentId);
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Agent {AgentId} assigned to live chat session {SessionId} successfully",
            request.AgentId, request.SessionId);

        return true;
    }
}
