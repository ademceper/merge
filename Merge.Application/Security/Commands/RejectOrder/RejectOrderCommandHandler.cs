using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Security.Commands.RejectOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class RejectOrderCommandHandler : IRequestHandler<RejectOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RejectOrderCommandHandler> _logger;

    public RejectOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<RejectOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        var verification = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == request.VerificationId, cancellationToken);

        if (verification == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        verification.Reject(request.VerifiedByUserId, request.Reason);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order rejected. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}, Reason: {Reason}",
            request.VerificationId, request.VerifiedByUserId, request.Reason);

        return true;
    }
}
