using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.Interfaces.User;
using Merge.Application.Services.Notification;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.ValueObjects;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using CartEntity = Merge.Domain.Entities.Cart;
using OrderEntity = Merge.Domain.Entities.Order;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Cart;
using AutoMapper;


namespace Merge.Application.Services.Cart;

public class PreOrderService : IPreOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;
    private readonly ILogger<PreOrderService> _logger;

    public PreOrderService(ApplicationDbContext context, IUnitOfWork unitOfWork, IEmailService emailService, IMapper mapper, ILogger<PreOrderService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PreOrderDto> CreatePreOrderAsync(Guid userId, CreatePreOrderDto dto)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PreOrder + Campaign update)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ✅ PERFORMANCE: AsNoTracking for read-only product query
            // ✅ PERFORMANCE: Removed manual !p.IsDeleted check (Global Query Filter handles it)
            var product = await _context.Set<ProductEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

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
                .FirstOrDefaultAsync();

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

        var preOrder = new PreOrder
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Quantity = dto.Quantity,
            Price = price,
            DepositAmount = depositAmount,
            ExpectedAvailabilityDate = campaign.ExpectedDeliveryDate,
            ExpiresAt = campaign.EndDate,
            VariantOptions = dto.VariantOptions,
            Notes = dto.Notes,
            Status = depositAmount > 0 ? PreOrderStatus.Pending : PreOrderStatus.Confirmed
        };

            await _context.Set<PreOrder>().AddAsync(preOrder);

            // Reload campaign for update (tracking gerekli)
            var campaignToUpdate = await _context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.Id == campaign.Id);
            
            if (campaignToUpdate == null)
            {
                throw new NotFoundException("Kampanya", campaign.Id);
            }

            campaignToUpdate.CurrentQuantity += dto.Quantity;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // ✅ ARCHITECTURE: Reload with Include for AutoMapper
            preOrder = await _context.Set<PreOrder>()
                .AsNoTracking()
                .Include(po => po.Product)
                .FirstOrDefaultAsync(po => po.Id == preOrder.Id);

            // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
            return _mapper.Map<PreOrderDto>(preOrder!);
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PreOrder olusturma hatasi. UserId: {UserId}, ProductId: {ProductId}",
                userId, dto.ProductId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<PreOrderDto?> GetPreOrderAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrder = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .FirstOrDefaultAsync(po => po.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return preOrder != null ? _mapper.Map<PreOrderDto>(preOrder) : null;
    }

    public async Task<IEnumerable<PreOrderDto>> GetUserPreOrdersAsync(Guid userId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrders = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .Where(po => po.UserId == userId)
            .OrderByDescending(po => po.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<PreOrderDto>>(preOrders);
    }

    public async Task<bool> CancelPreOrderAsync(Guid id, Guid userId)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PreOrder + Campaign update)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
            var preOrder = await _context.Set<PreOrder>()
                .FirstOrDefaultAsync(po => po.Id == id && po.UserId == userId);

            if (preOrder == null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Siparişe dönüştürülmüş bir ön sipariş iptal edilemez.");
            }

            preOrder.Status = PreOrderStatus.Cancelled;

            // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
            var campaign = await _context.Set<PreOrderCampaign>()
                .FirstOrDefaultAsync(c => c.ProductId == preOrder.ProductId);

            if (campaign != null)
            {
                campaign.CurrentQuantity -= preOrder.Quantity;
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PreOrder iptal hatasi. PreOrderId: {PreOrderId}, UserId: {UserId}",
                id, userId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<bool> PayDepositAsync(Guid userId, PayPreOrderDepositDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrder = await _context.Set<PreOrder>()
            .FirstOrDefaultAsync(po => po.Id == dto.PreOrderId && po.UserId == userId);

        if (preOrder == null) return false;

        if (preOrder.DepositPaid >= preOrder.DepositAmount)
        {
            throw new BusinessException("Depozito zaten ödenmiş.");
        }

        preOrder.DepositPaid += dto.Amount;

        if (preOrder.DepositPaid >= preOrder.DepositAmount)
        {
            preOrder.Status = PreOrderStatus.DepositPaid;
        }

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ConvertToOrderAsync(Guid preOrderId)
    {
        // ✅ ARCHITECTURE: Transaction başlat - atomic operation (PreOrder + Order + OrderItem)
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
            var preOrder = await _context.Set<PreOrder>()
                .Include(po => po.Product)
                .Include(po => po.User)
                .FirstOrDefaultAsync(po => po.Id == preOrderId);

            if (preOrder == null) return false;

            if (preOrder.Status == PreOrderStatus.Converted)
            {
                throw new BusinessException("Ön sipariş zaten dönüştürülmüş.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            // Kullanıcının default address'ini çek
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId && a.IsDefault);
            
            if (address == null)
            {
                // Default address yoksa ilk address'i al
                address = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == preOrder.UserId);
            }
            
            if (address == null)
            {
                throw new BusinessException("Sipariş oluşturmak için adres bilgisi gereklidir.");
            }

            // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullan
            var order = OrderEntity.Create(preOrder.UserId, address.Id, address);
            
            // Product'ı çek (AddItem için gerekli)
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == preOrder.ProductId);
            
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

            await _context.Set<OrderEntity>().AddAsync(order);

            preOrder.Status = PreOrderStatus.Converted;
            preOrder.ConvertedToOrderAt = DateTime.UtcNow;
            preOrder.OrderId = order.Id;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception ex)
        {
            // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve throw et
            _logger.LogError(ex,
                "PreOrder siparise donusturme hatasi. PreOrderId: {PreOrderId}",
                preOrderId);
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task NotifyPreOrderAvailableAsync(Guid preOrderId)
    {
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var preOrder = await _context.Set<PreOrder>()
            .Include(po => po.Product)
            .Include(po => po.User)
            .FirstOrDefaultAsync(po => po.Id == preOrderId);

        if (preOrder == null) return;

        if (preOrder.NotificationSentAt != null) return;

        await _emailService.SendEmailAsync(
            preOrder.User.Email ?? string.Empty,
            "Your Pre-Order is Ready!",
            $"Good news! Your pre-order for {preOrder.Product.Name} is now available and ready to ship."
        );

        preOrder.NotificationSentAt = DateTime.UtcNow;
        preOrder.ActualAvailabilityDate = DateTime.UtcNow;
        preOrder.Status = PreOrderStatus.ReadyToShip;

        await _unitOfWork.SaveChangesAsync();
    }

    // Campaigns
    public async Task<PreOrderCampaignDto> CreateCampaignAsync(CreatePreOrderCampaignDto dto)
    {
        var campaign = new PreOrderCampaign
        {
            Name = dto.Name,
            Description = dto.Description,
            ProductId = dto.ProductId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            MaxQuantity = dto.MaxQuantity,
            DepositPercentage = dto.DepositPercentage,
            SpecialPrice = dto.SpecialPrice
        };

        await _context.Set<PreOrderCampaign>().AddAsync(campaign);
        await _unitOfWork.SaveChangesAsync();

        // ✅ ARCHITECTURE: Reload with Include for AutoMapper
        campaign = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == campaign.Id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<PreOrderCampaignDto>(campaign!);
    }

    public async Task<PreOrderCampaignDto?> GetCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaign = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .FirstOrDefaultAsync(c => c.Id == id);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return campaign != null ? _mapper.Map<PreOrderCampaignDto>(campaign) : null;
    }

    public async Task<IEnumerable<PreOrderCampaignDto>> GetActiveCampaignsAsync()
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaigns = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.IsActive)
            .Where(c => c.StartDate <= now && c.EndDate >= now)
            .OrderBy(c => c.EndDate)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<PreOrderCampaignDto>>(campaigns);
    }

    public async Task<IEnumerable<PreOrderCampaignDto>> GetCampaignsByProductAsync(Guid productId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaigns = await _context.Set<PreOrderCampaign>()
            .AsNoTracking()
            .Include(c => c.Product)
            .Where(c => c.ProductId == productId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<IEnumerable<PreOrderCampaignDto>>(campaigns);
    }

    public async Task<bool> UpdateCampaignAsync(Guid id, CreatePreOrderCampaignDto dto)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaign = await _context.Set<PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        campaign.Name = dto.Name;
        campaign.Description = dto.Description;
        campaign.StartDate = dto.StartDate;
        campaign.EndDate = dto.EndDate;
        campaign.ExpectedDeliveryDate = dto.ExpectedDeliveryDate;
        campaign.MaxQuantity = dto.MaxQuantity;
        campaign.DepositPercentage = dto.DepositPercentage;
        campaign.SpecialPrice = dto.SpecialPrice;

        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivateCampaignAsync(Guid id)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var campaign = await _context.Set<PreOrderCampaign>()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (campaign == null) return false;

        campaign.IsActive = false;
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<PreOrderStatsDto> GetPreOrderStatsAsync()
    {
        // ✅ PERFORMANCE: Database'de aggregation yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var totalPreOrders = await _context.Set<PreOrder>()
            .CountAsync();

        var pendingPreOrders = await _context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Pending);

        var confirmedPreOrders = await _context.Set<PreOrder>()
            .CountAsync(po => po.Status == PreOrderStatus.Confirmed || po.Status == PreOrderStatus.DepositPaid);

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalRevenue = await _context.Set<PreOrder>()
            .SumAsync(po => po.Price * po.Quantity);

        var totalDeposits = await _context.Set<PreOrder>()
            .SumAsync(po => po.DepositPaid);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var recentPreOrders = await _context.Set<PreOrder>()
            .AsNoTracking()
            .Include(po => po.Product)
            .OrderByDescending(po => po.CreatedAt)
            .Take(10)
            .ToListAsync();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var recentDtos = _mapper.Map<IEnumerable<PreOrderDto>>(recentPreOrders).ToList();

        return new PreOrderStatsDto
        {
            TotalPreOrders = totalPreOrders,
            PendingPreOrders = pendingPreOrders,
            ConfirmedPreOrders = confirmedPreOrders,
            TotalRevenue = totalRevenue,
            TotalDeposits = totalDeposits,
            RecentPreOrders = recentDtos
        };
    }

    public async Task ProcessExpiredPreOrdersAsync()
    {
        var now = DateTime.UtcNow;
        // ✅ PERFORMANCE: Removed manual !po.IsDeleted check (Global Query Filter handles it)
        var expiredPreOrders = await _context.Set<PreOrder>()
            .Where(po => po.Status == PreOrderStatus.Pending && po.ExpiresAt < now)
            .ToListAsync();

        foreach (var preOrder in expiredPreOrders)
        {
            preOrder.Status = PreOrderStatus.Expired;
        }

        await _unitOfWork.SaveChangesAsync();
    }

}
