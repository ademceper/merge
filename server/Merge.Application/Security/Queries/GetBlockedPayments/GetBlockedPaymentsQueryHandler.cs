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

namespace Merge.Application.Security.Queries.GetBlockedPayments;

public class GetBlockedPaymentsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetBlockedPaymentsQueryHandler> logger) : IRequestHandler<GetBlockedPaymentsQuery, IEnumerable<PaymentFraudPreventionDto>>
{

    public async Task<IEnumerable<PaymentFraudPreventionDto>> Handle(GetBlockedPaymentsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Engellenen ödemeler sorgulanıyor");

        var checks = await context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .Where(c => c.IsBlocked)
            .OrderByDescending(c => c.RiskScore)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Engellenen ödemeler bulundu. Count: {Count}", checks.Count);

        return mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks);
    }
}
