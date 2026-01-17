using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.AcknowledgeAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class AcknowledgeAlertCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<AcknowledgeAlertCommandHandler> logger) : IRequestHandler<AcknowledgeAlertCommand, bool>
{

    public async Task<bool> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        alert.Acknowledge(request.AcknowledgedByUserId);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Security alert acknowledged. AlertId: {AlertId}, AcknowledgedByUserId: {AcknowledgedByUserId}",
            request.AlertId, request.AcknowledgedByUserId);

        return true;
    }
}
