using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.CancelPreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CancelPreOrderCommandHandler : IRequestHandler<CancelPreOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelPreOrderCommandHandler> _logger;

    public CancelPreOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<CancelPreOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelPreOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var preOrder = await _context.Set<Merge.Domain.Modules.Ordering.PreOrder>()
                .FirstOrDefaultAsync(po => po.Id == request.PreOrderId && po.UserId == request.UserId, cancellationToken);

            if (preOrder == null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Siparişe dönüştürülmüş bir ön sipariş iptal edilemez.");
            }

            preOrder.Cancel();

            var campaign = await _context.Set<Merge.Domain.Modules.Marketing.PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.ProductId == preOrder.ProductId, cancellationToken);

            if (campaign != null)
            {
                campaign.DecrementQuantity(preOrder.Quantity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "PreOrder iptal hatasi. PreOrderId: {PreOrderId}, UserId: {UserId}",
                request.PreOrderId, request.UserId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

