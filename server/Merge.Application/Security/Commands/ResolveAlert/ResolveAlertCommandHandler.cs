using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.ResolveAlert;

public class ResolveAlertCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ResolveAlertCommandHandler> logger) : IRequestHandler<ResolveAlertCommand, bool>
{

    public async Task<bool> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert is null) return false;

        alert.Resolve(request.ResolvedByUserId, request.ResolutionNotes);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security alert resolved. AlertId: {AlertId}, ResolvedByUserId: {ResolvedByUserId}",
            request.AlertId, request.ResolvedByUserId);

        return true;
    }
}
