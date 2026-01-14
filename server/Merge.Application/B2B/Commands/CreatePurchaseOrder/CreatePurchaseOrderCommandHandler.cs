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
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreatePurchaseOrder;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePurchaseOrderCommandHandler> _logger;
    private readonly B2BSettings _b2bSettings;

    public CreatePurchaseOrderCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreatePurchaseOrderCommandHandler> logger,
        IOptions<B2BSettings> b2bSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _b2bSettings = b2bSettings.Value;
    }

    public async Task<PurchaseOrderDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating purchase order. B2BUserId: {B2BUserId}, OrganizationId: {OrganizationId}",
            request.B2BUserId, request.Dto.OrganizationId);

        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PurchaseOrder + Items + Updates)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
            var b2bUser = await _context.Set<B2BUser>()
                .Include(b => b.Organization)
                .FirstOrDefaultAsync(b => b.Id == request.B2BUserId && b.IsApproved, cancellationToken);

            if (b2bUser == null)
            {
                throw new NotFoundException("B2B kullanıcı", Guid.Empty);
            }

            var poNumber = await GeneratePONumberAsync(cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
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

            await _context.Set<PurchaseOrder>().AddAsync(purchaseOrder, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // ✅ PERFORMANCE: Batch load all products at once (N+1 query fix)
            var productIds = request.Dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _context.Set<ProductEntity>()
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

            // ✅ PERFORMANCE: Batch load all wholesale prices at once (N+1 query fix)
            var now = DateTime.UtcNow;
            var wholesalePricesQuery = _context.Set<WholesalePrice>()
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

            // ✅ PERFORMANCE: Batch load all volume discounts at once (N+1 query fix)
            var volumeDiscountsQuery = _context.Set<VolumeDiscount>()
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

            // ✅ PERFORMANCE: Dictionary lookup için optimize et (O(1) lookup)
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
                                            (wp.MaxQuantity == null || wp.MaxQuantity >= itemDto.Quantity));

                    if (wholesalePrice != null)
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
                                            (vd.MaxQuantity == null || vd.MaxQuantity >= itemDto.Quantity));
                }
                
                if (discount == null)
                {
                    var categoryDiscountKey = new { ProductId = (Guid?)null, CategoryId = (Guid?)product.CategoryId };
                    if (volumeDiscountLookup.TryGetValue(categoryDiscountKey, out var categoryDiscounts))
                    {
                        discount = categoryDiscounts
                            .FirstOrDefault(vd => vd.MinQuantity <= itemDto.Quantity &&
                                                (vd.MaxQuantity == null || vd.MaxQuantity >= itemDto.Quantity));
                    }
                }

                if (discount != null && discount.DiscountPercentage > 0)
                {
                    unitPrice = unitPrice * (1 - discount.DiscountPercentage / 100);
                }

                // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
                // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
                var unitPriceMoney = new Merge.Domain.ValueObjects.Money(unitPrice);
                purchaseOrder.AddItem(
                    products[itemDto.ProductId],
                    itemDto.Quantity,
                    unitPriceMoney,
                    itemDto.Notes);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            // ✅ BOLUM 2.3: Hardcoded Values YASAK - Configuration kullan
            // ✅ BOLUM 1.3: Value Objects - Money value object kullanımı
            var subTotal = purchaseOrder.SubTotal;
            var taxAmount = new Merge.Domain.ValueObjects.Money(subTotal * _b2bSettings.DefaultTaxRate);
            purchaseOrder.SetTax(taxAmount);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ ARCHITECTURE: Reload with Include for AutoMapper
            // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple collection includes)
            purchaseOrder = await _context.Set<PurchaseOrder>()
                .AsNoTracking()
                .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
                .Include(po => po.Organization)
                .Include(po => po.B2BUser!)
                    .ThenInclude(b => b.User)
                .Include(po => po.ApprovedBy)
                .Include(po => po.CreditTerm)
                .Include(po => po.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(po => po.Id == purchaseOrder.Id, cancellationToken);

            _logger.LogInformation("Purchase order created successfully. PurchaseOrderId: {PurchaseOrderId}, PONumber: {PONumber}",
                purchaseOrder!.Id, purchaseOrder.PONumber);

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return _mapper.Map<PurchaseOrderDto>(purchaseOrder);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PurchaseOrder olusturma hatasi. B2BUserId: {B2BUserId}, OrganizationId: {OrganizationId}",
                request.B2BUserId, request.Dto.OrganizationId);
            // ✅ ARCHITECTURE: Hata olursa ROLLBACK - hiçbir şey yazılmaz
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<string> GeneratePONumberAsync(CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var existingCount = await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .CountAsync(po => po.PONumber.StartsWith($"PO-{date}"), cancellationToken);

        return $"PO-{date}-{(existingCount + 1):D6}";
    }
}

