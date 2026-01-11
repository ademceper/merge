using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using AddressEntity = Merge.Domain.Entities.Address;

namespace Merge.Application.Cart.Commands.ConvertPreOrderToOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class ConvertPreOrderToOrderCommandHandler : IRequestHandler<ConvertPreOrderToOrderCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConvertPreOrderToOrderCommandHandler> _logger;

    public ConvertPreOrderToOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<ConvertPreOrderToOrderCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(ConvertPreOrderToOrderCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var preOrder = await _context.Set<PreOrder>()
                .Include(po => po.Product)
                .Include(po => po.User)
                .FirstOrDefaultAsync(po => po.Id == request.PreOrderId, cancellationToken);

            if (preOrder == null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Ön sipariş zaten dönüştürülmüş.");
            }

            var address = await _context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId && a.IsDefault, cancellationToken);

            if (address == null)
            {
                address = await _context.Set<AddressEntity>()
                    .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId, cancellationToken);
            }

            if (address == null)
            {
                throw new BusinessException("Sipariş oluşturmak için adres bilgisi gereklidir.");
            }

            var order = Merge.Domain.Entities.Order.Create(preOrder.UserId, address.Id, address);

            var product = await _context.Set<Merge.Domain.Entities.Product>()
                .FirstOrDefaultAsync(p => p.Id == preOrder.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", preOrder.ProductId);
            }

            order.AddItem(product, preOrder.Quantity);

            var shippingCost = new Money(0);
            order.SetShippingCost(shippingCost);

            var tax = new Money(0);
            order.SetTax(tax);

            await _context.Set<Merge.Domain.Entities.Order>().AddAsync(order, cancellationToken);

            preOrder.ConvertToOrder(order.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "PreOrder siparise donusturme hatasi. PreOrderId: {PreOrderId}",
                request.PreOrderId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

