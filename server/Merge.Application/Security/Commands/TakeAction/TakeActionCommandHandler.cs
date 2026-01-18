using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.TakeAction;

public class TakeActionCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<TakeActionCommandHandler> logger) : IRequestHandler<TakeActionCommand, bool>
{

    public async Task<bool> Handle(TakeActionCommand request, CancellationToken cancellationToken)
    {
        var securityEvent = await context.Set<AccountSecurityEvent>()
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        if (securityEvent is null) return false;

        securityEvent.TakeAction(request.ActionTakenByUserId, request.Action, request.Notes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security event action alındı. EventId: {EventId}, Action: {Action}, ActionTakenByUserId: {ActionTakenByUserId}",
            request.EventId, request.Action, request.ActionTakenByUserId);

        return true;
    }
}
