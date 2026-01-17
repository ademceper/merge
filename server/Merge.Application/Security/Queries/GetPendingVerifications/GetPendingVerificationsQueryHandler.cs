using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetPendingVerifications;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetPendingVerificationsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetPendingVerificationsQueryHandler> logger) : IRequestHandler<GetPendingVerificationsQuery, IEnumerable<OrderVerificationDto>>
{

    public async Task<IEnumerable<OrderVerificationDto>> Handle(GetPendingVerificationsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        logger.LogInformation("Bekleyen order verification'lar sorgulanıyor");

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !v.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için Cartesian Explosion önleme
        var verifications = await context.Set<OrderVerification>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy)
            .Where(v => v.Status == VerificationStatus.Pending)
            .OrderByDescending(v => v.RequiresManualReview)
            .ThenByDescending(v => v.RiskScore)
            .ToListAsync(cancellationToken);

        logger.LogInformation("Bekleyen order verification'lar bulundu. Count: {Count}", verifications.Count);

        return mapper.Map<IEnumerable<OrderVerificationDto>>(verifications);
    }
}
