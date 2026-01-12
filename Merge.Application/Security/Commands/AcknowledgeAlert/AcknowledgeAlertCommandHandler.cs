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
public class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AcknowledgeAlertCommandHandler> _logger;

    public AcknowledgeAlertCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<AcknowledgeAlertCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        alert.Acknowledge(request.AcknowledgedByUserId);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Security alert acknowledged. AlertId: {AlertId}, AcknowledgedByUserId: {AcknowledgedByUserId}",
            request.AlertId, request.AcknowledgedByUserId);

        return true;
    }
}
