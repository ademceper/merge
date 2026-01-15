using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Cart.Commands.CreatePreOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreatePreOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreatePreOrderCommandHandler> logger) : IRequestHandler<CreatePreOrderCommand, PreOrderDto>
{

    public async Task<PreOrderDto> Handle(CreatePreOrderCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var product = await context.Set<Merge.Domain.Modules.Catalog.Product>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (product is null)
            {
                throw new NotFoundException("Ürün", request.ProductId);
            }

            var campaign = await context.Set<PreOrderCampaign>()
                .AsNoTracking()
                .Where(c => c.ProductId == request.ProductId && c.IsActive)
                .Where(c => c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (campaign is null)
            {
                throw new BusinessException("Bu ürün için aktif ön sipariş kampanyası yok.");
            }

            if (campaign.MaxQuantity > 0 && campaign.CurrentQuantity >= campaign.MaxQuantity)
            {
                throw new BusinessException("Ön sipariş kampanyası dolu.");
            }

            var price = campaign.SpecialPrice > 0 ? campaign.SpecialPrice : product.Price;
            var depositAmount = price * (campaign.DepositPercentage / 100);

            // ✅ BOLUM 1.1: Rich Domain Model - User entity'yi yükle (PreOrder.Create için gerekli)
            // ✅ User entity'si BaseEntity'den türemediği için IDbContext.Users property'si kullanılıyor
            var user = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException("Kullanıcı", request.UserId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
            // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
            var priceMoney = new Merge.Domain.ValueObjects.Money(price);
            var depositAmountMoney = new Merge.Domain.ValueObjects.Money(depositAmount);
            var preOrder = PreOrder.Create(
                request.UserId,
                request.ProductId,
                product,
                user,
                request.Quantity,
                priceMoney,
                depositAmountMoney,
                campaign.ExpectedDeliveryDate,
                campaign.EndDate,
                request.Notes,
                request.VariantOptions);

            if (depositAmount == 0)
            {
                preOrder.Confirm();
            }

            await context.Set<PreOrder>().AddAsync(preOrder, cancellationToken);

            var campaignToUpdate = await context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

            // ✅ BOLUM 7.1.6: Pattern Matching - Null pattern matching
            if (campaignToUpdate is null)
            {
                throw new NotFoundException("Kampanya", campaign.Id);
            }

            campaignToUpdate.IncrementQuantity(request.Quantity);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            preOrder = await context.Set<PreOrder>()
                .AsNoTracking()
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.Id == preOrder.Id, cancellationToken);

            return mapper.Map<PreOrderDto>(preOrder!);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "PreOrder olusturma hatasi. UserId: {UserId}, ProductId: {ProductId}",
                request.UserId, request.ProductId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}

