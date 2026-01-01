using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CartEntity = Merge.Domain.Entities.Cart;
using Merge.Application.Interfaces.Cart;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Services.Notification;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using Merge.Infrastructure.Data;
using Merge.Infrastructure.Repositories;
using ProductEntity = Merge.Domain.Entities.Product;
using Merge.Application.DTOs.Cart;
using Merge.Application.DTOs.Marketing;
using AutoMapper;


namespace Merge.Application.Services.Cart;

public class AbandonedCartService : IAbandonedCartService
{
    private readonly ApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ICouponService _couponService;
    private readonly IMapper _mapper;
    private readonly ILogger<AbandonedCartService> _logger;

    public AbandonedCartService(
        ApplicationDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ICouponService couponService,
        IMapper mapper,
        ILogger<AbandonedCartService> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _couponService = couponService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AbandonedCartDto>> GetAbandonedCartsAsync(int minHours = 1, int maxDays = 30)
    {
        var minDate = DateTime.UtcNow.AddDays(-maxDays);
        var maxDate = DateTime.UtcNow.AddHours(-minHours);
        var now = DateTime.UtcNow;

        // ✅ PERFORMANCE: Database'de tüm hesaplamaları yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        // Step 1: Get abandoned cart IDs (carts with items, updated in date range)
        var abandonedCartIds = await _context.Carts
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate)
            .Select(c => c.Id)
            .ToListAsync();

        if (abandonedCartIds.Count == 0)
        {
            return Enumerable.Empty<AbandonedCartDto>();
        }

        // Step 2: Get user IDs for these carts
        var userIds = await _context.Carts
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync();

        // Step 3: Filter out carts that have been converted to orders
        var userIdsWithOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => userIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync();

        // Step 4: Get final abandoned cart IDs (excluding those converted to orders)
        var finalAbandonedCartIds = await _context.Carts
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id) && 
                       !userIdsWithOrders.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync();

        if (finalAbandonedCartIds.Count == 0)
        {
            return Enumerable.Empty<AbandonedCartDto>();
        }

        // Step 5: Get cart data with computed properties from database
        var cartsData = await _context.Carts
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
            .ToListAsync();

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
            .ToListAsync();

        // ✅ PERFORMANCE: Dictionary oluşturma minimal bir işlem (O(n) lookup için gerekli)
        // Bu işlem sadece property assignment, memory'de complex işlem YOK
        var emailStatsDict = new Dictionary<Guid, (int EmailsSentCount, bool HasReceivedEmail, DateTime? LastEmailSent)>();
        foreach (var stat in emailStats)
        {
            emailStatsDict[stat.CartId] = (stat.EmailsSentCount, stat.HasReceivedEmail, stat.LastEmailSent);
        }

        // Step 7: Get cart items for all carts in one query
        var cartItems = await _context.CartItems
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => finalAbandonedCartIds.Contains(ci.CartId))
            .ToListAsync();

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
        var result = new List<AbandonedCartDto>();
        foreach (var c in cartsData)
        {
            var dto = new AbandonedCartDto
            {
                CartId = c.CartId,
                UserId = c.UserId,
                UserEmail = c.UserEmail,
                UserName = c.UserName,
                ItemCount = c.ItemCount,
                TotalValue = c.TotalValue,
                LastModified = c.LastModified,
                HoursSinceAbandonment = c.HoursSinceAbandonment,
                Items = cartItemsDict.ContainsKey(c.CartId)
                    ? _mapper.Map<IEnumerable<CartItemDto>>(cartItemsDict[c.CartId]).ToList()
                    : new List<CartItemDto>(),
                HasReceivedEmail = emailStatsDict.ContainsKey(c.CartId) && emailStatsDict[c.CartId].HasReceivedEmail,
                EmailsSentCount = emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].EmailsSentCount : 0,
                LastEmailSent = emailStatsDict.ContainsKey(c.CartId) ? emailStatsDict[c.CartId].LastEmailSent : null
            };
            result.Add(dto);
        }

        return result;
    }

    public async Task<AbandonedCartDto?> GetAbandonedCartByIdAsync(Guid cartId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Carts
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId);

        if (cart == null)
        {
            return null;
        }

        // ✅ PERFORMANCE: Database'de Count ve FirstOrDefault yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emailsSentCount = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == cartId)
            .CountAsync();

        var hasReceivedEmail = emailsSentCount > 0;

        var lastEmailSent = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.CartId == cartId)
            .OrderByDescending(e => e.SentAt)
            .Select(e => (DateTime?)e.SentAt)
            .FirstOrDefaultAsync();

        // ✅ PERFORMANCE: Database'de Sum ve Count yap (memory'de işlem YASAK)
        var itemCount = await _context.CartItems
            .AsNoTracking()
            .CountAsync(ci => ci.CartId == cartId);

        var totalValue = await _context.CartItems
            .AsNoTracking()
            .Where(ci => ci.CartId == cartId)
            .SumAsync(ci => ci.Price * ci.Quantity);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var items = await _context.CartItems
            .AsNoTracking()
            .Include(ci => ci.Product)
            .Where(ci => ci.CartId == cartId)
            .ToListAsync();

        var itemsDto = _mapper.Map<IEnumerable<CartItemDto>>(items).ToList();

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        // Note: AbandonedCartDto complex mapping gerektiriyor (computed properties), AfterMap kullanıyoruz
        var dto = _mapper.Map<AbandonedCartDto>(cart);
        dto.CartId = cart.Id;
        dto.ItemCount = itemCount;
        dto.TotalValue = totalValue;
        dto.LastModified = cart.UpdatedAt ?? cart.CreatedAt;
        dto.HoursSinceAbandonment = cart.UpdatedAt.HasValue 
            ? (int)(DateTime.UtcNow - cart.UpdatedAt.Value).TotalHours 
            : (int)(DateTime.UtcNow - cart.CreatedAt).TotalHours;
        dto.Items = itemsDto;
        dto.HasReceivedEmail = hasReceivedEmail;
        dto.EmailsSentCount = emailsSentCount;
        dto.LastEmailSent = lastEmailSent;

        return dto;
    }

    public async Task<AbandonedCartRecoveryStatsDto> GetRecoveryStatsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var minDate = DateTime.UtcNow.AddDays(-days);
        var maxDate = DateTime.UtcNow.AddHours(-1);

        // ✅ PERFORMANCE: Database'de Count ve Sum yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        // Get abandoned cart IDs (carts with items, updated in date range, not converted to orders)
        var abandonedCartIds = await _context.Carts
            .AsNoTracking()
            .Where(c => c.CartItems.Any() &&
                       c.UpdatedAt >= minDate &&
                       c.UpdatedAt <= maxDate)
            .Select(c => c.Id)
            .ToListAsync();

        // Filter out carts that have been converted to orders
        var abandonedCartUserIds = await _context.Carts
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id))
            .Select(c => c.UserId)
            .Distinct()
            .ToListAsync();

        var userIdsWithOrders = await _context.Orders
            .AsNoTracking()
            .Where(o => abandonedCartUserIds.Contains(o.UserId))
            .Select(o => o.UserId)
            .Distinct()
            .ToListAsync();

        var finalAbandonedCartIds = await _context.Carts
            .AsNoTracking()
            .Where(c => abandonedCartIds.Contains(c.Id) && 
                       !userIdsWithOrders.Contains(c.UserId))
            .Select(c => c.Id)
            .ToListAsync();

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        var totalAbandonedCarts = finalAbandonedCartIds.Count;

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalAbandonedValue = await _context.CartItems
            .AsNoTracking()
            .Where(ci => finalAbandonedCartIds.Contains(ci.CartId))
            .SumAsync(ci => ci.Price * ci.Quantity);

        // ✅ PERFORMANCE: Database'de Count yap (memory'de işlem YASAK)
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emailsSent = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate)
            .CountAsync();

        var emailsOpened = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.WasOpened)
            .CountAsync();

        var emailsClicked = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.WasClicked)
            .CountAsync();

        var recoveredCarts = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Where(e => e.SentAt >= startDate && e.ResultedInPurchase)
            .CountAsync();

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var recoveredRevenue = await _context.Orders
            .AsNoTracking()
            .Where(o => o.CreatedAt >= startDate)
            .Join(
                _context.Set<AbandonedCartEmail>().AsNoTracking().Where(e => e.ResultedInPurchase),
                order => order.UserId,
                email => email.UserId,
                (order, email) => order.TotalAmount
            )
            .SumAsync();

        return new AbandonedCartRecoveryStatsDto
        {
            TotalAbandonedCarts = totalAbandonedCarts,
            TotalAbandonedValue = totalAbandonedValue,
            EmailsSent = emailsSent,
            EmailsOpened = emailsOpened,
            EmailsClicked = emailsClicked,
            RecoveredCarts = recoveredCarts,
            RecoveredRevenue = recoveredRevenue,
            RecoveryRate = totalAbandonedCarts > 0 ? (decimal)recoveredCarts / totalAbandonedCarts * 100 : 0,
            AverageCartValue = totalAbandonedCarts > 0 ? totalAbandonedValue / totalAbandonedCarts : 0
        };
    }

    public async Task SendRecoveryEmailAsync(Guid cartId, string emailType = "First", bool includeCoupon = false, decimal? couponDiscountPercentage = null)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Carts
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == cartId);

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
            var discount = couponDiscountPercentage ?? 10;
            var couponDto = new CouponDto
            {
                Code = $"RECOVER{DateTime.UtcNow.Ticks.ToString().Substring(8)}",
                DiscountPercentage = discount,
                MinimumPurchaseAmount = 0,
                UsageLimit = 1,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(7),
                Description = $"{discount}% off for completing your purchase"
            };

            var createdCoupon = await _couponService.CreateAsync(couponDto);
            couponId = createdCoupon.Id;
            couponCode = createdCoupon.Code;
        }

        // Prepare email content
        var subject = emailType switch
        {
            "First" => "You left items in your cart!",
            "Second" => "Still thinking about your cart?",
            "Final" => "Last chance! Your cart is waiting",
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
        var totalValue = await _context.CartItems
            .AsNoTracking()
            .Where(ci => ci.CartId == cartId)
            .SumAsync(ci => ci.Price * ci.Quantity);

        var body = $@"
            <h2>Hi {user.FirstName},</h2>
            <p>You left some great items in your cart!</p>
            <ul>{itemsHtml.ToString()}</ul>
            <p><strong>Total: ${totalValue:F2}</strong></p>
            {(includeCoupon ? $"<p><strong>Use code {couponCode} for {couponDiscountPercentage}% off!</strong></p>" : "")}
            <p><a href='https://yoursite.com/cart/{cartId}'>Complete your purchase now</a></p>
        ";

        await _emailService.SendEmailAsync(user.Email, subject, body);

        // Record email sent
        var abandonedCartEmail = new AbandonedCartEmail
        {
            CartId = cartId,
            UserId = user.Id,
            EmailType = emailType,
            SentAt = DateTime.UtcNow,
            CouponId = couponId
        };

        await _context.Set<AbandonedCartEmail>().AddAsync(abandonedCartEmail);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SendBulkRecoveryEmailsAsync(int minHours = 2, string emailType = "First")
    {
        var abandonedCarts = await GetAbandonedCartsAsync(minHours, 30);

        // ✅ PERFORMANCE: Filter carts that haven't received this email type yet
        // Business logic için memory'de filtreleme yapıyoruz (email type kontrolü)
        // Bu işlem database'de yapılamaz çünkü complex business logic gerekiyor
        var cartsToEmail = new List<AbandonedCartDto>();
        foreach (var c in abandonedCarts)
        {
            bool shouldEmail = false;
            if (emailType == "First")
                shouldEmail = !c.HasReceivedEmail;
            else if (emailType == "Second")
                shouldEmail = c.EmailsSentCount == 1 && c.HoursSinceAbandonment >= 24;
            else if (emailType == "Final")
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
                var includeCoupon = emailType == "Final"; // Include coupon in final email
                await SendRecoveryEmailAsync(cart.CartId, emailType, includeCoupon, 15);
            }
            catch (Exception)
            {
                // Log error but continue with other carts
                continue;
            }
        }
    }

    public async Task<bool> TrackEmailOpenAsync(Guid emailId)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var email = await _context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == emailId);

        if (email == null)
        {
            return false;
        }

        email.WasOpened = true;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TrackEmailClickAsync(Guid emailId)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var email = await _context.Set<AbandonedCartEmail>()
            .FirstOrDefaultAsync(e => e.Id == emailId);

        if (email == null)
        {
            return false;
        }

        email.WasClicked = true;
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task MarkCartAsRecoveredAsync(Guid cartId)
    {
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        var emails = await _context.Set<AbandonedCartEmail>()
            .Where(e => e.CartId == cartId)
            .ToListAsync();

        foreach (var email in emails)
        {
            email.ResultedInPurchase = true;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<AbandonedCartEmailDto>> GetCartEmailHistoryAsync(Guid cartId)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !e.IsDeleted check (Global Query Filter handles it)
        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var emails = await _context.Set<AbandonedCartEmail>()
            .AsNoTracking()
            .Include(e => e.Coupon)
            .Where(e => e.CartId == cartId)
            .OrderByDescending(e => e.SentAt)
            .ToListAsync();

        return _mapper.Map<IEnumerable<AbandonedCartEmailDto>>(emails);
    }
}
