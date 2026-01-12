using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.User;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using AddressEntity = Merge.Domain.Modules.Identity.Address;
using Merge.Application.DTOs.Cart;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Cart;

public class PreOrderService : IPreOrderService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<PreOrderService> _logger;

    public PreOrderService(IDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, ILogger<PreOrderService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PreOrderDto> CreatePreOrderAsync(Guid userId, CreatePreOrderDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PreOrder + Campaign update)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ PERFORMANCE: AsNoTracking for read-only product query
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
            var product = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId, cancellationToken);

            if (product == null)
            {
                throw new NotFoundException("Ürün", dto.ProductId);
            }

            // ✅ PERFORMANCE: AsNoTracking for read-only campaign query
            // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
            var campaign = await _context.Set<PreOrderCampaign>()
                .AsNoTracking()
                .Where(c => c.ProductId == dto.ProductId && c.IsActive)
                .Where(c => c.StartDate <= DateTime.UtcNow && c.EndDate >= DateTime.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

        if (campaign == null)
        {
            throw new BusinessException("Bu ürün için aktif ön sipariş kampanyası yok.");
        }

        if (campaign.MaxQuantity > 0 && campaign.CurrentQuantity >= campaign.MaxQuantity)
        {
            throw new BusinessException("Ön sipariş kampanyası dolu.");
        }

        var price = campaign.SpecialPrice > 0 ? campaign.SpecialPrice : product.Price;
        var depositAmount = price * (campaign.DepositPercentage / 100);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        var preOrder = PreOrder.Create(
            userId,
            dto.ProductId,
            dto.Quantity,
            price,
            depositAmount,
            campaign.ExpectedDeliveryDate,
            campaign.EndDate,
            dto.Notes,
            dto.VariantOptions);

        // Eğer depozito yoksa direkt confirmed yap
        if (depositAmount == 0)
        {
            preOrder.Confirm();
        }

            await _context.Set<PreOrder>().AddAsync(preOrder, cancellationToken);

            // Reload campaign for update (tracking gerekli)
            var campaignToUpdate = await _context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);
            
            if (campaignToUpdate == null)
            {
                throw new NotFoundException("Kampanya", campaign.Id);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            campaignToUpdate.IncrementQuantity(dto.Quantity);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // ✅ ARCHITECTURE: Reload with Include for AutoMapper
            preOrder = await _context.Set<PreOrder>()
                .AsNoTracking()
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.Id == preOrder.Id, cancellationToken);

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return _mapper.Map<PreOrderDto>(preOrder!);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PreOrder olusturma hatasi. UserId: {UserId}, ProductId: {ProductId}",
                userId, dto.ProductId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PreOrderDto?> GetPreOrderAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrder = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .FirstOrDefaultAsync(po => po.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return preOrder != null ? _mapper.Map<PreOrderDto>(preOrder) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<PreOrderDto>> GetUserPreOrdersAsync(Guid userId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .Where(po => po.UserId == userId);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var preOrders = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PreOrderDto>>(preOrders);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PreOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> CancelPreOrderAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PreOrder + Campaign update)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
            var preOrder = await _context.Set<PreOrder>()
                .FirstOrDefaultAsync(po => po.Id == id && po.UserId == userId, cancellationToken);

            if (preOrder == null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Siparişe dönüştürülmüş bir ön sipariş iptal edilemez.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            preOrder.Cancel();

            // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
            var campaign = await _context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.ProductId == preOrder.ProductId, cancellationToken);

            if (campaign != null)
            {
                // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
                campaign.DecrementQuantity(preOrder.Quantity);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PreOrder iptal hatasi. PreOrderId: {PreOrderId}, UserId: {UserId}",
                id, userId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> PayDepositAsync(Guid userId, PayPreOrderDepositDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrder = await _context.Set<PreOrder>()
            .FirstOrDefaultAsync(po => po.Id == dto.PreOrderId && po.UserId == userId, cancellationToken);

        if (preOrder == null) return false;

        if (preOrder.DepositPaid >= preOrder.DepositAmount)
        {
            throw new BusinessException("Depozito zaten ödenmiş.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        preOrder.PayDeposit(dto.Amount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> ConvertToOrderAsync(Guid preOrderId, CancellationToken cancellationToken = default)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PreOrder + Order + OrderItem)
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
            var preOrder = await _context.Set<PreOrder>()
                .Include(po => po.Product)
                .Include(po => po.User)
                .FirstOrDefaultAsync(po => po.Id == preOrderId, cancellationToken);

            if (preOrder == null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Ön sipariş zaten dönüştürülmüş.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            // Kullanıcının default address'ini çek
            var address = await _context.Set<AddressEntity>()
                .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId && a.IsDefault, cancellationToken);

            if (address == null)
            {
                // Default address yoksa ilk address'i al
                address = await _context.Set<AddressEntity>()
                    .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId, cancellationToken);
            }
            
            if (address == null)
            {
                throw new BusinessException("Sipariş oluşturmak için adres bilgisi gereklidir.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var order = OrderEntity.Create(preOrder.UserId, address.Id, address);
            
            // Product'ı çek (AddItem için gerekli)
            var product = await _context.Set<ProductEntity>()
                .FirstOrDefaultAsync(p => p.Id == preOrder.ProductId, cancellationToken);
            
            if (product == null)
            {
                throw new NotFoundException("Ürün", preOrder.ProductId);
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullan
            order.AddItem(product, preOrder.Quantity);
            
            // Shipping ve tax hesapla
            var shippingCost = new Money(0); // Pre-order için shipping cost 0
            order.SetShippingCost(shippingCost);
            
            var tax = new Money(0); // Pre-order için tax 0
            order.SetTax(tax);

            await _context.Set<OrderEntity>().AddAsync(order, cancellationToken);

            // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
            preOrder.ConvertToOrder(order.Id);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PreOrder siparise donusturme hatasi. PreOrderId: {PreOrderId}",
                preOrderId);
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task NotifyPreOrderAvailableAsync(Guid preOrderId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrder = await _context.Set<PreOrder>()
            .Include(po => po.Product)
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.Id == preOrderId, cancellationToken);

        if (preOrder == null) return;

        if (preOrder.NotificationSentAt != null) return;

        // ✅ NOTE: IEmailService interface'inde CancellationToken yok, bu domain dışında
        await _emailService.SendEmailAsync(
            preOrder.User.Email ?? string.Empty,
            "Your Pre-Order is Ready!",
            $"Good news! Your pre-order for {preOrder.Product.Name} is now available and ready to ship."
        );

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        preOrder.MarkNotificationAsSent();
        preOrder.MarkAsReadyToShip();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // Campaigns
    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PreOrderCampaignDto> CreateCampaignAsync(CreatePreOrderCampaignDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        var campaign = PreOrderCampaign.Create(
            dto.Name,
            dto.Description,
            dto.ProductId,
            dto.StartDate,
            dto.EndDate,
            dto.ExpectedDeliveryDate,
            dto.MaxQuantity,
            dto.DepositPercentage,
            dto.SpecialPrice,
            dto.NotifyOnAvailable);

        await _context.Set<PreOrderCampaign>().AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        campaign = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<PreOrderCampaignDto>(campaign!);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PreOrderCampaignDto?> GetCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaign = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return campaign != null ? _mapper.Map<PreOrderCampaignDto>(campaign) : null;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<PreOrderCampaignDto>> GetActiveCampaignsAsync(int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.IsActive)
            .Where(c => c.StartDate <= now && c.EndDate >= now);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderBy(c => c.EndDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PreOrderCampaignDto>>(campaigns);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PreOrderCampaignDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<PreOrderCampaignDto>> GetCampaignsByProductAsync(Guid productId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.ProductId == productId);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var campaigns = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<PreOrderCampaignDto>>(campaigns);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<PreOrderCampaignDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> UpdateCampaignAsync(Guid id, CreatePreOrderCampaignDto dto, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaign = await _context.Set<PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        campaign.UpdateBasicInfo(dto.Name, dto.Description, dto.MaxQuantity);
        campaign.UpdateDates(dto.StartDate, dto.EndDate, dto.ExpectedDeliveryDate);
        campaign.UpdatePricing(dto.DepositPercentage, dto.SpecialPrice);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> DeactivateCampaignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaign = await _context.Set<PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (campaign == null) return false;

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        campaign.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<PreOrderStatsDto> GetPreOrderStatsAsync(CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var totalPreOrders = await _context.Set<PreOrder>()
            .CountAsync(cancellationToken);

        var pendingPreOrders = await _context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Pending, cancellationToken);

        var confirmedPreOrders = await _context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Confirmed || po.Status == PreOrderStatus.DepositPaid, cancellationToken);

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalRevenue = await _context.Set<PreOrder>()
            .SumAsync(po => po.Price * po.Quantity, cancellationToken);

        var totalDeposits = await _context.Set<PreOrder>()
            .SumAsync(po => po.DepositPaid, cancellationToken);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var recentPreOrders = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .OrderByDescending(po => po.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var recentDtos = _mapper.Map<IEnumerable<PreOrderDto>>(recentPreOrders).ToList();

        return new PreOrderStatsDto(
            totalPreOrders,
            pendingPreOrders,
            confirmedPreOrders,
            totalRevenue,
            totalDeposits,
            recentDtos
        );
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task ProcessExpiredPreOrdersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var expiredPreOrders = await _context.Set<PreOrder>()
            .Where(po => po.Status == PreOrderStatus.Pending && po.ExpiresAt < now)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        foreach (var preOrder in expiredPreOrders)
        {
            preOrder.MarkAsExpired();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

}
