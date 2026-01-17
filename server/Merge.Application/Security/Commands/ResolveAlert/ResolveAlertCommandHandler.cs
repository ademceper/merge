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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ResolveAlertCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<ResolveAlertCommandHandler> logger) : IRequestHandler<ResolveAlertCommand, bool>
{

    public async Task<bool> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        alert.Resolve(request.ResolvedByUserId, request.ResolutionNotes);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security alert resolved. AlertId: {AlertId}, ResolvedByUserId: {ResolvedByUserId}",
            request.AlertId, request.ResolvedByUserId);

        return true;
    }
}
