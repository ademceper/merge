using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetCheckByPaymentId;

public class GetCheckByPaymentIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetCheckByPaymentIdQueryHandler> logger) : IRequestHandler<GetCheckByPaymentIdQuery, PaymentFraudPreventionDto?>
{

    public async Task<PaymentFraudPreventionDto?> Handle(GetCheckByPaymentIdQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Payment fraud check sorgulanıyor. PaymentId: {PaymentId}", request.PaymentId);

        var check = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .FirstOrDefaultAsync(c => c.PaymentId == request.PaymentId, cancellationToken);

        if (check is null)
        {
            logger.LogWarning("Payment fraud check bulunamadı. PaymentId: {PaymentId}", request.PaymentId);
            return null;
        }

        logger.LogInformation("Payment fraud check bulundu. CheckId: {CheckId}, PaymentId: {PaymentId}",
            check.Id, request.PaymentId);

        return mapper.Map<PaymentFraudPreventionDto>(check);
    }
}
