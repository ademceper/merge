using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Security.Commands.BlockPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class BlockPaymentCommandHandler : IRequestHandler<BlockPaymentCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BlockPaymentCommandHandler> _logger;

    public BlockPaymentCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<BlockPaymentCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(BlockPaymentCommand request, CancellationToken cancellationToken)
    {
        var check = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == request.CheckId, cancellationToken);

        if (check == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        check.Block(request.Reason);
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment blocked. CheckId: {CheckId}, Reason: {Reason}", request.CheckId, request.Reason);

        return true;
    }
}
