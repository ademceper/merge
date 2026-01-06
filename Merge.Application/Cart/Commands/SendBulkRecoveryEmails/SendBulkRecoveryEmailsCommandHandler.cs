using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Cart.Queries.GetAbandonedCarts;
using Merge.Application.Cart.Commands.SendRecoveryEmail;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Enums;

namespace Merge.Application.Cart.Commands.SendBulkRecoveryEmails;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class SendBulkRecoveryEmailsCommandHandler : IRequestHandler<SendBulkRecoveryEmailsCommand>
{
    private readonly IMediator _mediator;
    private readonly ILogger<SendBulkRecoveryEmailsCommandHandler> _logger;
    private readonly CartSettings _cartSettings;

    public SendBulkRecoveryEmailsCommandHandler(
        IMediator mediator,
        ILogger<SendBulkRecoveryEmailsCommandHandler> logger,
        IOptions<CartSettings> cartSettings)
    {
        _mediator = mediator;
        _logger = logger;
        _cartSettings = cartSettings.Value;
    }

    public async Task Handle(SendBulkRecoveryEmailsCommand request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 2.0: MediatR + CQRS pattern - GetAbandonedCartsQuery dispatch
        var abandonedCartsQuery = new GetAbandonedCartsQuery(request.MinHours, 30, 1, 1000);
        var abandonedCarts = await _mediator.Send(abandonedCartsQuery, cancellationToken);

        // ✅ PERFORMANCE: Filter carts that haven't received this email type yet
        // ✅ BOLUM 1.2: Enum Kullanimi (ZORUNLU - String Status YASAK)
        var cartsToEmail = new List<AbandonedCartDto>();
        foreach (var c in abandonedCarts.Items)
        {
            bool shouldEmail = false;
            if (request.EmailType == AbandonedCartEmailType.First)
                shouldEmail = !c.HasReceivedEmail;
            else if (request.EmailType == AbandonedCartEmailType.Second)
                shouldEmail = c.EmailsSentCount == 1 && c.HoursSinceAbandonment >= 24;
            else if (request.EmailType == AbandonedCartEmailType.Final)
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
                var includeCoupon = request.EmailType == AbandonedCartEmailType.Final;
                var sendEmailCommand = new SendRecoveryEmailCommand(
                    cart.CartId,
                    request.EmailType,
                    includeCoupon,
                    _cartSettings.DefaultAbandonedCartCouponDiscount);
                
                // ✅ BOLUM 2.0: MediatR + CQRS pattern - SendRecoveryEmailCommand dispatch
                await _mediator.Send(sendEmailCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                // ✅ BOLUM 2.1: Exception ASLA yutulmamali - logla ve devam et
                _logger.LogError(ex,
                    "Abandoned cart recovery email gonderilemedi. CartId: {CartId}, EmailType: {EmailType}",
                    cart.CartId, request.EmailType);
                // Continue with other carts - don't fail entire batch
                continue;
            }
        }
    }
}

