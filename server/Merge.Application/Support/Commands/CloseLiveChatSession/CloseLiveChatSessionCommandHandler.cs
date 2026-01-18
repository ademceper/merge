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

namespace Merge.Application.Support.Commands.CloseLiveChatSession;

public class CloseLiveChatSessionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<CloseLiveChatSessionCommandHandler> logger) : IRequestHandler<CloseLiveChatSessionCommand, bool>
{

    public async Task<bool> Handle(CloseLiveChatSessionCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Closing live chat session {SessionId}", request.SessionId);

        var session = await context.Set<LiveChatSession>()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session == null)
        {
            logger.LogWarning("Live chat session {SessionId} not found for closure", request.SessionId);
            throw new NotFoundException("Canlı sohbet oturumu", request.SessionId);
        }

        session.Close();
        
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Live chat session {SessionId} closed successfully", request.SessionId);

        return true;
    }
}
