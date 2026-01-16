using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using PreOrder = Merge.Domain.Modules.Ordering.PreOrder;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.PayPreOrderDeposit;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class PayPreOrderDepositCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork) : IRequestHandler<PayPreOrderDepositCommand, bool>
{

    public async Task<bool> Handle(PayPreOrderDepositCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await context.Set<PreOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.PreOrderId && po.UserId == request.UserId, cancellationToken);

        // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
        if (preOrder is null) return false;

        if (preOrder.DepositPaid >= preOrder.DepositAmount)
        {
            throw new BusinessException("Depozito zaten ödenmiş.");
        }

        var depositAmountMoney = new Money(request.Amount);
        preOrder.PayDeposit(depositAmountMoney);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

