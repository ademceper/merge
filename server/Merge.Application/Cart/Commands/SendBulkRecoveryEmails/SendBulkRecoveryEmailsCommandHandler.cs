using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Merge.Application.Cart.Queries.GetAbandonedCarts;
using Merge.Application.Cart.Commands.SendRecoveryEmail;
using Merge.Application.Configuration;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Commands.SendBulkRecoveryEmails;

public class SendBulkRecoveryEmailsCommandHandler(
    IMediator mediator,
    ILogger<SendBulkRecoveryEmailsCommandHandler> logger,
    IOptions<CartSettings> cartSettings) : IRequestHandler<SendBulkRecoveryEmailsCommand>
{

    public async Task Handle(SendBulkRecoveryEmailsCommand request, CancellationToken cancellationToken)
    {
        var abandonedCartsQuery = new GetAbandonedCartsQuery(request.MinHours, 30, 1, 1000);
        var abandonedCarts = await mediator.Send(abandonedCartsQuery, cancellationToken);

        List<AbandonedCartDto> cartsToEmail = [];
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
                var includeCoupon = request.EmailType == AbandonedCartEmailType.Final;
                var sendEmailCommand = new SendRecoveryEmailCommand(
                    cart.CartId,
                    request.EmailType,
                    includeCoupon,
                    cartSettings.Value.DefaultAbandonedCartCouponDiscount);
                
                await mediator.Send(sendEmailCommand, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Abandoned cart recovery email gonderilemedi. CartId: {CartId}, EmailType: {EmailType}",
                    cart.CartId, request.EmailType);
                // Continue with other carts - don't fail entire batch
                continue;
            }
        }
    }
}

