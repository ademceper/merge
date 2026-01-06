using MediatR;
using Microsoft.EntityFrameworkCore;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;

namespace Merge.Application.Cart.Commands.PayPreOrderDeposit;

public class PayPreOrderDepositCommandHandler : IRequestHandler<PayPreOrderDepositCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PayPreOrderDepositCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(PayPreOrderDepositCommand request, CancellationToken cancellationToken)
    {
        var preOrder = await _context.Set<Domain.Entities.PreOrder>()
            .FirstOrDefaultAsync(po => po.Id == request.PreOrderId && po.UserId == request.UserId, cancellationToken);

        if (preOrder == null) return false;

        if (preOrder.DepositPaid >= preOrder.DepositAmount)
        {
            throw new BusinessException("Depozito zaten ödenmiş.");
        }

        preOrder.PayDeposit(request.Amount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}

