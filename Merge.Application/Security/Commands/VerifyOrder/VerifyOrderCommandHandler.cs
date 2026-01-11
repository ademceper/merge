using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Security.Commands.VerifyOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class VerifyOrderCommandHandler : IRequestHandler<VerifyOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VerifyOrderCommandHandler> _logger;

    public VerifyOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<VerifyOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(VerifyOrderCommand request, CancellationToken cancellationToken)
    {
        var verification = await _context.Set<OrderVerification>()
            .FirstOrDefaultAsync(v => v.Id == request.VerificationId, cancellationToken);

        if (verification == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        verification.Verify(request.VerifiedByUserId, request.Notes);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order verified. VerificationId: {VerificationId}, VerifiedByUserId: {VerifiedByUserId}",
            request.VerificationId, request.VerifiedByUserId);

        return true;
    }
}
