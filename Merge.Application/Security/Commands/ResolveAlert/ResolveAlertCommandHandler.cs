using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Security.Commands.ResolveAlert;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ResolveAlertCommandHandler : IRequestHandler<ResolveAlertCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ResolveAlertCommandHandler> _logger;

    public ResolveAlertCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ResolveAlertCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _context.Set<SecurityAlert>()
            .FirstOrDefaultAsync(a => a.Id == request.AlertId, cancellationToken);

        if (alert == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        alert.Resolve(request.ResolvedByUserId, request.ResolutionNotes);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Security alert resolved. AlertId: {AlertId}, ResolvedByUserId: {ResolvedByUserId}",
            request.AlertId, request.ResolvedByUserId);

        return true;
    }
}
