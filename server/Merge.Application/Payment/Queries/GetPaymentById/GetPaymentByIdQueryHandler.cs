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

namespace Merge.Application.Payment.Queries.GetPaymentById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullaniyor (Service layer bypass)
public class GetPaymentByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPaymentByIdQueryHandler> logger) : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Retrieving payment with ID: {PaymentId}", request.PaymentId);

        var payment = await context.Set<PaymentEntity>()
            .AsNoTracking()
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

        if (payment is null)
        {
            logger.LogWarning("Payment not found with ID: {PaymentId}", request.PaymentId);
            return null;
        }

        return mapper.Map<PaymentDto>(payment);
    }
}
