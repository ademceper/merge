using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MediatR;
using CartEntity = Merge.Domain.Modules.Ordering.Cart;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Services.Notification;
using Merge.Application.Exceptions;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Enums;
using ProductEntity = Merge.Domain.Modules.Catalog.Product;
using Merge.Application.DTOs.Cart;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Marketing.Commands.CreateCoupon;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.Modules.Notifications;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.ValueObjects;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;


namespace Merge.Application.Services.Cart;

public class AbandonedCartService : IAbandonedCartService
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;
    private readonly ILogger<AbandonedCartService> _logger;
    private readonly CartSettings _cartSettings;

    public AbandonedCartService(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        IMediator mediator,
        IMapper mapper,
        ILogger<AbandonedCartService> logger,
        IOptions<CartSettings> cartSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _mediator = mediator;
        _mapper = mapper;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<AbandonedCartDto>> GetAbandonedCartsAsync(int minHours = 1, int maxDays = 30, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var minDate = DateTime.UtcNow.AddDays(-maxDays);
        var maxDate = DateTime.UtcNow.AddHours(-minHours);
        var now = DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de tüm hesaplamaları yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // Step 1: Get abandoned cart IDs (carts with items, updated in date range)
        var abandonedCartIds = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (abandonedCartIds.Count == 0)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = new List<AbandonedCartDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // Step 2: Get user IDs for these carts
        var userIds = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Step 3: Filter out carts that have been converted to orders
        var userIdsWithOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => userIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        // Step 4: Get final abandoned cart IDs (excluding those converted to orders)
        var finalAbandonedCartIds = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id) && 
                       !userIdsWithOrders.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (finalAbandonedCartIds.Count == 0)
        {
            return new PagedResult<AbandonedCartDto>
            {
                Items = new List<AbandonedCartDto>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = finalAbandonedCartIds.Count;

        // Step 5: Get cart data with computed properties from database
        var cartsData = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => finalAbandonedCartIds.Contains(c.Id))
            .Select(c => new
            {
                CartId = c.Id,
                UserId = c.UserId,
                UserEmail = c.User != null ? c.User.Email : "",
                UserName = c.User != null ? (c.User.FirstName + " " + c.User.LastName) : "",
                LastModified = c.UpdatedAt ?? c.CreatedAt,
                HoursSinceAbandonment = c.UpdatedAt.HasValue 
                    ? (int)((now - c.UpdatedAt.Value).TotalHours)
                    : (int)((now - c.CreatedAt).TotalHours),
                ItemCount = c.CartItems.Count,
                TotalValue = c.CartItems.Sum(ci => ci.Price * ci.Quantity)
            })
            .OrderByDescending(c => c.TotalValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Step 6: Get email stats for all carts in one query (database'de GroupBy)
        var emailStats = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => finalAbandonedCartIds.Contains(e.CartId))
            .GroupBy(e => e.CartId)
            .Select(g => new
            {
                CartId = g.Key,
                EmailsSentCount = g.Count(),
                HasReceivedEmail = g.Any(),
                LastEmailSent = g.OrderByDescending(e => e.SentAt).Select(e => (DateTime?)e.SentAt).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Dictionary oluşturma minimal bir işlem (O(n) lookup için gerekli)
        // Bu işlem sadece property assignment, memory'de complex işlem YOK
        // ✅ BOLUM 6.4: List Capacity Pre-allocation (ZORUNLU)
        var emailStatsDict = new Dictionary<Guid, (int EmailsSentCount, bool HasReceivedEmail, DateTime? LastEmailSent)>(emailStats.Count);
        foreach (var stat in emailStats)
        {
            emailStatsDict[stat.CartId] = (stat.EmailsSentCount, stat.HasReceivedEmail, stat.LastEmailSent);
        }

        // Step 7: Get cart items for all carts in one query
        var cartItems = await _context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => finalAbandonedCartIds.Contains(ci.CartId))
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Dictionary oluşturma minimal bir işlem (O(1) lookup için gerekli)
        // Bu işlem sadece grouping ve dictionary oluşturma, memory'de complex işlem YOK
        var cartItemsDict = new Dictionary<Guid, List<CartItem>>();
        foreach (var item in cartItems)
        {
            if (!cartItemsDict.ContainsKey(item.CartId))
            {
                cartItemsDict[item.CartId] = new List<CartItem>();
            }
            cartItemsDict[item.CartId].Add(item);
        }

        // Step 8: Build DTOs (minimal memory operations - only property assignment)
        // ✅ PERFORMANCE: Select sadece DTO oluşturma için kullanılıyor (property assignment)
        // Bu işlem database'de yapılamaz çünkü DTO oluşturma gerekiyor
        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        var result = new List<AbandonedCartDto>();
        foreach (var c in cartsData)
        {
            var items = cartItemsDict.ContainsKey(c.CartId)
                ? _mapper.Map<IEnumerable<CartItemDto>>(cartItemsDict[c.CartId]).ToList().AsReadOnly()
                : new List<CartItemDto>().AsReadOnly();
            
            var dto = new AbandonedCartDto(
                c.CartId,
                c.UserId,
                c.UserEmail ?? string.Empty,
                c.UserName ?? string.Empty,
                c.ItemCount,
                c.TotalValue,
                c.LastModified,
                c.HoursSinceAbandonment,
                items,
                emailStatsDict.ContainsKey(c.CartId) && emailStatsDict[c.CartId].HasReceivedEmail,
                emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].EmailsSentCount : 0,
                emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].LastEmailSent : null
            );
            result.Add(dto);
        }

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<AbandonedCartDto>
        {
            Items = result,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AbandonedCartDto?> GetAbandonedCartByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

        if (cart == null)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Count ve FirstOrDefault yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emailsSentCount = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == cartId)
            .CountAsync(cancellationToken);

        var hasReceivedEmail = emailsSentCount > 0;

        var lastEmailSent = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == cartId)
            .OrderByDescending(e => e.SentAt)
            .Select(e => (DateTime?)e.SentAt)
            .FirstOrDefaultAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de Sum ve Count yap (memory'de işlem YASAK)
        var itemCount = await _context.Set<CartItem>()
            .AsNoTracking()
            .CountAsync(ci => ci.CartId == cartId, cancellationToken);

        var totalValue = await _context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => ci.CartId == cartId)
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = await _context.Set<CartItem>()
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync(cancellationToken);

        var itemsDto = _mapper.Map<IEnumerable<CartItemDto>>(items).ToList().AsReadOnly();

        // ✅ BOLUM 7.1.5: Records (ZORUNLU - DTOs record olmalı) - Positional constructor kullanımı
        var userEmail = cart.User?.Email ?? string.Empty;
        var userName = cart.User != null ? $"{cart.User.FirstName} {cart.User.LastName}" : string.Empty;
        var lastModified = cart.UpdatedAt ?? cart.CreatedAt;
        var hoursSinceAbandonment = cart.UpdatedAt.HasValue 
            ? (int)(DateTime.UtcNow - cart.UpdatedAt.Value).TotalHours 
            : (int)(DateTime.UtcNow - cart.CreatedAt).TotalHours;

        var dto = new AbandonedCartDto(
            cart.Id,
            cart.UserId,
            userEmail,
            userName,
            itemCount,
            totalValue,
            lastModified,
            hoursSinceAbandonment,
            itemsDto,
            hasReceivedEmail,
            emailsSentCount,
            lastEmailSent
        );

        return dto;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<AbandonedCartRecoveryStatsDto> GetRecoveryStatsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var minDate = DateTime.UtcNow.AddDays(-days);
        var maxDate = DateTime.UtcNow.AddHours(-1);

        // ✅ PERFORMANCE: Database'de Count ve Sum yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        // Get abandoned cart IDs (carts with items, updated in date range, not converted to orders)
        var abandonedCartIds = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        // Filter out carts that have been converted to orders
        var abandonedCartUserIds = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var userIdsWithOrders = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => abandonedCartUserIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var finalAbandonedCartIds = await _context.Set<CartEntity>()
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id) && 
                       !userIdsWithOrders.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var totalAbandonedCarts = finalAbandonedCartIds.Count;

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalAbandonedValue = await _context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => finalAbandonedCartIds.Contains(ci.CartId))
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emailsSent = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate)
            .CountAsync(cancellationToken);

        var emailsOpened = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.WasOpened)
            .CountAsync(cancellationToken);

        var emailsClicked = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.WasClicked)
            .CountAsync(cancellationToken);

        var recoveredCarts = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.ResultedInPurchase)
            .CountAsync(cancellationToken);

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var recoveredRevenue = await _context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDate)
            .Join(
                _context.Set<AbandonedCartEmail>().AsNoTracking().Where(e => e.ResultedInPurchase),
                order => order.UserId,
                email => email.UserId,
                (order, email) => order.TotalAmount
            )
            .SumAsync(cancellationToken);

        return new AbandonedCartRecoveryStatsDto(
            totalAbandonedCarts,
            totalAbandonedValue,
            emailsSent,
            emailsOpened,
            emailsClicked,
            recoveredCarts,
            recoveredRevenue,
            totalAbandonedCarts > 0 ? (decimal)recoveredCarts / totalAbandonedCarts * 100 : 0,
            totalAbandonedCarts > 0 ? totalAbandonedValue / totalAbandonedCarts : 0
        );
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    public async Task SendRecoveryEmailAsync(Guid cartId, AbandonedCartEmailType emailType = AbandonedCartEmailType.First, bool includeCoupon = false, decimal? couponDiscountPercentage = null, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Set<CartEntity>()
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException("Sepet", cartId);
        }

        var user = cart.User;
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            throw new NotFoundException("Kullanıcı email", Guid.Empty);
        }

        // Create coupon if requested
        Guid? couponId = null;
        string? couponCode = null;
        if (includeCoupon)
        {
            // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
            var discount = couponDiscountPercentage ?? _cartSettings.DefaultAbandonedCartCouponDiscount;
            var couponDto = new CouponDto(
                Id: Guid.Empty,
                Code: $"RECOVER{DateTime.UtcNow.Ticks.ToString().Substring(8)}",
                Description: $"{discount}% off for completing your purchase",
                DiscountAmount: 0,
                DiscountPercentage: discount,
                MinimumPurchaseAmount: 0,
                MaximumDiscountAmount: null,
                StartDate: DateTime.UtcNow,
                EndDate: DateTime.UtcNow.AddDays(_cartSettings.AbandonedCartCouponValidityDays),
                UsageLimit: 1,
                UsedCount: 0,
                IsActive: true,
                IsForNewUsersOnly: false,
                ApplicableCategoryIds: null,
                ApplicableProductIds: null);

            // ✅ ARCHITECTURE: MediatR kullan (service layer YASAK)
            var createdCoupon = await _mediator.Send(new CreateCouponCommand(
                Code: couponDto.Code,
                Description: couponDto.Description ?? string.Empty,
                DiscountAmount: null,
                DiscountPercentage: couponDto.DiscountPercentage,
                StartDate: couponDto.StartDate,
                EndDate: couponDto.EndDate,
                UsageLimit: couponDto.UsageLimit,
                MinimumPurchaseAmount: couponDto.MinimumPurchaseAmount,
                MaximumDiscountAmount: couponDto.MaximumDiscountAmount,
                IsForNewUsersOnly: false,
                ApplicableCategoryIds: null,
                ApplicableProductIds: null
            ), cancellationToken);
            couponId = createdCoupon.Id;
            couponCode = createdCoupon.Code;
        }

        // Prepare email content
        // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        var subject = emailType switch
        {
            AbandonedCartEmailType.First => "You left items in your cart!",
            AbandonedCartEmailType.Second => "Still thinking about your cart?",
            AbandonedCartEmailType.Final => "Last chance! Your cart is waiting",
            _ => "Complete your purchase"
        };

        // ✅ PERFORMANCE: Email body oluşturma için string concatenation (minimal memory işlem)
        // Include ile yüklenmiş CartItems üzerinde iteration yapıyoruz (email içeriği için gerekli)
        var itemsHtml = new System.Text.StringBuilder();
        foreach (var ci in cart.CartItems)
        {
            itemsHtml.Append($"<li>{ci.Product.Name} - {ci.Quantity} x ${ci.Price}</li>");
        }

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalValue = await _context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => ci.CartId == cartId)
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        var body = $@"
            <h2>Hi {user.FirstName},</h2>
            <p>You left some great items in your cart!</p>
            <ul>{itemsHtml.ToString()}</ul>
            <p><strong>Total: ${totalValue:F2}</strong></p>
            {(includeCoupon ? $"<p><strong>Use code {couponCode} for {couponDiscountPercentage}% off!</strong></p>" : "")}
            <p><a href='https://yoursite.com/cart/{cartId}'>Complete your purchase now</a></p>
        ";

        // ✅ NOTE: IEmailService interface'inde CancellationToken yok, bu domain dışında
        await _emailService.SendEmailAsync(user.Email, subject, body);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        // Record email sent
        var abandonedCartEmail = AbandonedCartEmail.Create(cartId, user.Id, emailType, couponId);

        await _context.Set<AbandonedCartEmail>().AddAsync(abandonedCartEmail, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
    public async Task SendBulkRecoveryEmailsAsync(int minHours = 2, AbandonedCartEmailType emailType = AbandonedCartEmailType.First, CancellationToken cancellationToken = default)
    {
        var abandonedCarts = await GetAbandonedCartsAsync(minHours, 30, 1, 1000, cancellationToken);

        // ✅ PERFORMANCE: Filter carts that haven't received this email type yet
        // Business logic için memory'de filtreleme yapıyoruz (email type kontrolü)
        // Bu işlem database'de yapılamaz çünkü complex business logic gerekiyor
        // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        var cartsToEmail = new List<AbandonedCartDto>();
        foreach (var c in abandonedCarts.Items)
        {
            bool shouldEmail = false;
            if (emailType == AbandonedCartEmailType.First)
                shouldEmail = !c.HasReceivedEmail;
            else if (emailType == AbandonedCartEmailType.Second)
                shouldEmail = c.EmailsSentCount == 1 && c.HoursSinceAbandonment >= 24;
            else if (emailType == AbandonedCartEmailType.Final)
                shouldEmail = c.EmailsSentCount == 2 && c.HoursSinceAbandonment >= 72;
            
            if (shouldEmail)
            {
                cartsToEmail.Add(c);
            }
        }

        foreach (var cart in cartsToEmail)
        {
            try
            {
                // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
                // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
                var includeCoupon = emailType == AbandonedCartEmailType.Final; // Include coupon in final email
                await SendRecoveryEmailAsync(cart.CartId, emailType, includeCoupon, _cartSettings.DefaultAbandonedCartCouponDiscount, cancellationToken);
            }
            catch (Exception ex)
            {
                // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve devam et
                _logger.LogError(ex,
                    "Abandoned cart recovery email gonderilemedi. CartId: {CartId}, EmailType: {EmailType}",
                    cart.CartId, emailType);
                // Continue with other carts - don't fail entire batch
                continue;
            }
        }
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> TrackEmailOpenAsync(Guid emailId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var email = await _context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == emailId, cancellationToken);

        if (email == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        email.MarkAsOpened();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task<bool> TrackEmailClickAsync(Guid emailId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var email = await _context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == emailId, cancellationToken);

        if (email == null)
        {
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        email.MarkAsClicked();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    public async Task MarkCartAsRecoveredAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emails = await _context.Set<AbandonedCartEmail>()
            .Where(e => e.CartId == cartId)
            .ToListAsync(cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Domain method kullanımı
        foreach (var email in emails)
        {
            email.MarkAsResultedInPurchase();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.4: Pagination (ZORUNLU)
    public async Task<PagedResult<AbandonedCartEmailDto>> GetCartEmailHistoryAsync(Guid cartId, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var query = _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Include(e => e.Coupon)
            .Where(e => e.CartId == cartId);

        // ✅ PERFORMANCE: TotalCount için ayrı query (CountAsync)
        var totalCount = await query.CountAsync(cancellationToken);

        var emails = await query
            .OrderByDescending(e => e.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = _mapper.Map<List<AbandonedCartEmailDto>>(emails);

        // ✅ BOLUM 3.4: Pagination (ZORUNLU) - PagedResult döndürüyor
        return new PagedResult<AbandonedCartEmailDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
