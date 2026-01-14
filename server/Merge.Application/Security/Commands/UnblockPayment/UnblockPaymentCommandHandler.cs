using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Commands.UnblockPayment;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class UnblockPaymentCommandHandler : IRequestHandler<UnblockPaymentCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnblockPaymentCommandHandler> _logger;

    public UnblockPaymentCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UnblockPaymentCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UnblockPaymentCommand request, CancellationToken cancellationToken)
    {
        var check = await _context.Set<PaymentFraudPrevention>()
            .FirstOrDefaultAsync(c => c.Id == request.CheckId, cancellationToken);

        if (check == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain Method kullanımı
        check.Unblock();
        // ✅ ARCHITECTURE: Domain event'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment unblocked. CheckId: {CheckId}", request.CheckId);

        return true;
    }
}
