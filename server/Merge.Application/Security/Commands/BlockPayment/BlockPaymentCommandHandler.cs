using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.BlockPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class BlockPaymentCommandHandler(IDbContext context, IUnitOfWork unitOfWork, ILogger<BlockPaymentCommandHandler> logger) : IRequestHandler<BlockPaymentCommand, bool>
{

    public async Task<bool> Handle(BlockPaymentCommand request, CancellationToken cancellationToken)
    {
        var check = await context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == request.CheckId, cancellationToken);

        if (check == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        check.Block(request.Reason);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Payment blocked. CheckId: {CheckId}, Reason: {Reason}", request.CheckId, request.Reason);

        return true;
    }
}
