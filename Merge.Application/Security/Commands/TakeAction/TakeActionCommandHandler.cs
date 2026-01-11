using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Security.Commands.TakeAction;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class TakeActionCommandHandler : IRequestHandler<TakeActionCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TakeActionCommandHandler> _logger;

    public TakeActionCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<TakeActionCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(TakeActionCommand request, CancellationToken cancellationToken)
    {
        var securityEvent = await _context.Set<AccountSecurityEvent>()
            .FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

        if (securityEvent == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        securityEvent.TakeAction(request.ActionTakenByUserId, request.Action, request.Notes);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Security event action alındı. EventId: {EventId}, Action: {Action}, ActionTakenByUserId: {ActionTakenByUserId}",
            request.EventId, request.Action, request.ActionTakenByUserId);

        return true;
    }
}
