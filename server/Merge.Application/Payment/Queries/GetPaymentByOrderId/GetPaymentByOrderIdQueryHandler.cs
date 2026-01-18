using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using PaymentEntity = Merge.Domain.Modules.Payment.Payment;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.GetPaymentByOrderId;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetPaymentByOrderIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPaymentByOrderIdQueryHandler> logger) : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{

    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving payment for order ID: {OrderId}", request.OrderId);

        var payment = await context.Set<PaymentEntity>()
            .AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.OrderId == request.OrderId, cancellationToken);

        if (payment == null)
        {
            logger.LogWarning("Payment not found for order ID: {OrderId}", request.OrderId);
            return null;
        }

        return mapper.Map<PaymentDto>(payment);
    }
}
