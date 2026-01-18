using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Payment;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Payment.Queries.GetPaymentMethodByCode;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetPaymentMethodByCodeQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPaymentMethodByCodeQueryHandler> logger) : IRequestHandler<GetPaymentMethodByCodeQuery, PaymentMethodDto?>
{

    public async Task<PaymentMethodDto?> Handle(GetPaymentMethodByCodeQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving payment method by code. Code: {Code}", request.Code);

        var paymentMethod = await context.Set<PaymentMethod>()
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.Code == request.Code && pm.IsActive, cancellationToken);

        if (paymentMethod == null)
        {
            logger.LogWarning("Payment method not found. Code: {Code}", request.Code);
            return null;
        }

        return mapper.Map<PaymentMethodDto>(paymentMethod);
    }
}
