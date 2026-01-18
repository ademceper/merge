using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using AutoMapper;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using CategoryEntity = Merge.Domain.Modules.Catalog.Category;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreatePurchaseOrder;

public class CreatePurchaseOrderCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreatePurchaseOrderCommandHandler> logger,
    IOptions<B2BSettings> b2bSettings) : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{

    public async Task<PurchaseOrderDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating purchase order. B2BUserId: {B2BUserId}, OrganizationId: {OrganizationId}",
            request.B2BUserId, request.Dto.OrganizationId);

        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var b2bUser = await context.Set<B2BUser>()
                .Include(b => b.Organization)
                .FirstOrDefaultAsync(b => b.Id == request.B2BUserId && b.IsApproved, cancellationToken);

            if (b2bUser is null)
            {
                throw new NotFoundException("B2B kullanıcı", Guid.Empty);
            }

            var poNumber = await GeneratePONumberAsync(cancellationToken);

            var purchaseOrder = PurchaseOrder.Create(
                request.Dto.OrganizationId,
                request.B2BUserId,
                poNumber,
                b2bUser.Organization,
                request.Dto.ExpectedDeliveryDate,
                request.Dto.CreditTermId);

            if (!string.IsNullOrWhiteSpace(request.Dto.Notes))
            {
                purchaseOrder.UpdateNotes(request.Dto.Notes);
            }

            await context.Set<PurchaseOrder>().AddAsync(purchaseOrder, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var productIds = request.Dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await context.Set<ProductEntity>()
                .AsNoTracking()
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, cancellationToken);

            // Validate all products exist
            foreach (var itemDto in request.Dto.Items)
            {
                if (!products.ContainsKey(itemDto.ProductId))
                {
                    throw new NotFoundException("Ürün", itemDto.ProductId);
                }
            }

            var now = DateTime.UtcNow;
            var wholesalePricesQuery = context.Set<WholesalePrice>()
                .AsNoTracking()
                .Where(wp => productIds.Contains(wp.ProductId) &&
                           wp.IsActive &&
                           (wp.StartDate == null || wp.StartDate <= now) &&
                           (wp.EndDate == null || wp.EndDate >= now));

            if (request.Dto.OrganizationId != Guid.Empty)
            {
                wholesalePricesQuery = wholesalePricesQuery.Where(wp => 
                    wp.OrganizationId == request.Dto.OrganizationId || wp.OrganizationId == null);
            }
            else
            {
                wholesalePricesQuery = wholesalePricesQuery.Where(wp => wp.OrganizationId == null);
            }

            var wholesalePrices = await wholesalePricesQuery.ToListAsync(cancellationToken);

            var volumeDiscountsQuery = context.Set<VolumeDiscount>()
                .AsNoTracking()
                .Where(vd => (productIds.Contains(vd.ProductId) || vd.CategoryId != null) &&
                           vd.IsActive &&
                           (vd.StartDate == null || vd.StartDate <= now) &&
                           (vd.EndDate == null || vd.EndDate >= now));

            if (request.Dto.OrganizationId != Guid.Empty)
            {
                volumeDiscountsQuery = volumeDiscountsQuery.Where(vd => 
                    vd.OrganizationId == request.Dto.OrganizationId || vd.OrganizationId == null);
            }
            else
            {
                volumeDiscountsQuery = volumeDiscountsQuery.Where(vd => vd.OrganizationId == null);
            }

            var volumeDiscounts = await volumeDiscountsQuery.ToListAsync(cancellationToken);

            var wholesalePriceLookup = wholesalePrices
                .GroupBy(wp => wp.ProductId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(wp => wp.MinQuantity).ToList()
                );

            var volumeDiscountLookup = volumeDiscounts
                .GroupBy(vd => new { 
                    ProductId = vd.ProductId != Guid.Empty ? (Guid?)vd.ProductId : null, 
                    CategoryId = vd.CategoryId 
                })
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(vd => vd.MinQuantity).ToList()
                );

            foreach (var itemDto in request.Dto.Items)
            {
                var product = products[itemDto.ProductId];

                // Get wholesale price if available
                var unitPrice = product.Price;
                if (wholesalePriceLookup.TryGetValue(product.Id, out var productWholesalePrices))
                {
                    var wholesalePrice = productWholesalePrices
                        .FirstOrDefault(wp => wp.MinQuantity <= itemDto.Quantity &&
                                            (wp.MaxQuantity is null || wp.MaxQuantity >= itemDto.Quantity));

                    if (wholesalePrice is not null)
                    {
                        unitPrice = wholesalePrice.Price;
                    }
                }

                // Apply volume discount
                VolumeDiscount? discount = null;
                
                var productDiscountKey = new { ProductId = (Guid?)product.Id, CategoryId = (Guid?)null };
                if (volumeDiscountLookup.TryGetValue(productDiscountKey, out var productDiscounts))
                {
                    discount = productDiscounts
                        .FirstOrDefault(vd => vd.MinQuantity <= itemDto.Quantity &&
                                            (vd.MaxQuantity is null || vd.MaxQuantity >= itemDto.Quantity));
                }
                
                if (discount is null)
                {
                    var categoryDiscountKey = new { ProductId = (Guid?)null, CategoryId = (Guid?)product.CategoryId };
                    if (volumeDiscountLookup.TryGetValue(categoryDiscountKey, out var categoryDiscounts))
                    {
                        discount = categoryDiscounts
                            .FirstOrDefault(vd => vd.MinQuantity <= itemDto.Quantity &&
                                                (vd.MaxQuantity is null || vd.MaxQuantity >= itemDto.Quantity));
                    }
                }

                if (discount is not null && discount.DiscountPercentage > 0)
                {
                    unitPrice = unitPrice * (1 - discount.DiscountPercentage / 100);
                }

                var unitPriceMoney = new Money(unitPrice);
                purchaseOrder.AddItem(
                    products[itemDto.ProductId],
                    itemDto.Quantity,
                    unitPriceMoney,
                    itemDto.Notes);
            }

            var subTotal = purchaseOrder.SubTotal;
            var taxAmount = new Money(subTotal * b2bSettings.Value.DefaultTaxRate);
            purchaseOrder.SetTax(taxAmount);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            purchaseOrder = await context.Set<PurchaseOrder>()
                .AsNoTracking()
                .Include(po => po.Organization)
                .Include(po => po.B2BUser!)
                    .ThenInclude(b => b.User)
                .Include(po => po.ApprovedBy)
                .Include(po => po.CreditTerm)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id, cancellationToken);

            logger.LogInformation("Purchase order created successfully. PurchaseOrderId: {PurchaseOrderId}, PONumber: {PONumber}",
                purchaseOrder!.Id, purchaseOrder.PONumber);

            return mapper.Map<PurchaseOrderDto>(purchaseOrder);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "PurchaseOrder olusturma hatasi. B2BUserId: {B2BUserId}, OrganizationId: {OrganizationId}",
                request.B2BUserId, request.Dto.OrganizationId);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GeneratePONumberAsync(CancellationToken cancellationToken)
    {
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var existingCount = await context.Set<PurchaseOrder>()
            .AsNoTracking()
            .CountAsync(po => po.PONumber.StartsWith($"PO-{date}"), cancellationToken);

        return $"PO-{date}-{(existingCount + 1):D6}";
    }
}

