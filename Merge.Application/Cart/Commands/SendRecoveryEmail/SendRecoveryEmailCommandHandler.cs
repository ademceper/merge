using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Interfaces;
using Merge.Application.Interfaces.Marketing;
using Merge.Application.Services.Notification;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Marketing;
using Merge.Domain.Entities;
using Merge.Domain.Enums;

namespace Merge.Application.Cart.Commands.SendRecoveryEmail;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SendRecoveryEmailCommandHandler : IRequestHandler<SendRecoveryEmailCommand>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ICouponService _couponService;
    private readonly ILogger<SendRecoveryEmailCommandHandler> _logger;
    private readonly CartSettings _cartSettings;

    public SendRecoveryEmailCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ICouponService couponService,
        ILogger<SendRecoveryEmailCommandHandler> logger,
        IOptions<CartSettings> cartSettings)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _couponService = couponService;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    public async Task Handle(SendRecoveryEmailCommand request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: Removed manual !c.IsDeleted check (Global Query Filter handles it)
        var cart = await _context.Set<Merge.Domain.Entities.Cart>()
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken);

        if (cart == null)
        {
            throw new NotFoundException("Sepet", request.CartId);
        }

        var user = cart.User;
        if (user == null || string.IsNullOrEmpty(user.Email))
        {
            throw new NotFoundException("Kullanıcı email", Guid.Empty);
        }

        // Create coupon if requested
        Guid? couponId = null;
        string? couponCode = null;
        if (request.IncludeCoupon)
        {
            // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
            var discount = request.CouponDiscountPercentage ?? _cartSettings.DefaultAbandonedCartCouponDiscount;
            var couponDto = new CouponDto
            {
                Code = $"RECOVER{DateTime.UtcNow.Ticks.ToString().Substring(8)}",
                DiscountPercentage = discount,
                MinimumPurchaseAmount = 0,
                UsageLimit = 1,
                IsActive = true,
                StartDate = DateTime.UtcNow,
                // ✅ BOLUM 2.3: Hardcoded Values YASAK (Configuration Kullan)
                EndDate = DateTime.UtcNow.AddDays(_cartSettings.AbandonedCartCouponValidityDays),
                Description = $"{discount}% off for completing your purchase"
            };

            var createdCoupon = await _couponService.CreateAsync(couponDto, cancellationToken);
            couponId = createdCoupon.Id;
            couponCode = createdCoupon.Code;
        }

        // Prepare email content
        // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        var subject = request.EmailType switch
        {
            AbandonedCartEmailType.First => "You left items in your cart!",
            AbandonedCartEmailType.Second => "Still thinking about your cart?",
            AbandonedCartEmailType.Final => "Last chance! Your cart is waiting",
            _ => "Complete your purchase"
        };

        // ✅ PERFORMANCE: Email body oluşturma için string concatenation (minimal memory işlem)
        var itemsHtml = new System.Text.StringBuilder();
        foreach (var ci in cart.CartItems)
        {
            itemsHtml.Append($"<li>{ci.Product.Name} - {ci.Quantity} x ${ci.Price}</li>");
        }

        // ✅ PERFORMANCE: Database'de Sum yap (memory'de işlem YASAK)
        var totalValue = await _context.Set<CartItem>()
            .AsNoTracking()
            .Where(ci => ci.CartId == request.CartId)
            .SumAsync(ci => ci.Price * ci.Quantity, cancellationToken);

        var body = $@"
            <h2>Hi {user.FirstName},</h2>
            <p>You left some great items in your cart!</p>
            <ul>{itemsHtml.ToString()}</ul>
            <p><strong>Total: ${totalValue:F2}</strong></p>
            {(request.IncludeCoupon ? $"<p><strong>Use code {couponCode} for {request.CouponDiscountPercentage}% off!</strong></p>" : "")}
            <p><a href='https://yoursite.com/cart/{request.CartId}'>Complete your purchase now</a></p>
        ";

        // ✅ NOTE: IEmailService interface'inde CancellationToken var
        await _emailService.SendEmailAsync(user.Email, subject, body, true, cancellationToken);

        // ✅ BOLUM 1.1: Rich Domain Model - Factory method kullanımı
        var abandonedCartEmail = AbandonedCartEmail.Create(request.CartId, user.Id, request.EmailType, couponId);

        await _context.Set<AbandonedCartEmail>().AddAsync(abandonedCartEmail, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

