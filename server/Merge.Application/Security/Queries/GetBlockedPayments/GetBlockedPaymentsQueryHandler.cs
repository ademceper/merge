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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetBlockedPaymentsQueryHandler : IRequestHandler<GetBlockedPaymentsQuery, IEnumerable<PaymentFraudPreventionDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetBlockedPaymentsQueryHandler> _logger;

    public GetBlockedPaymentsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetBlockedPaymentsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<PaymentFraudPreventionDto>> Handle(GetBlockedPaymentsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)
        _logger.LogInformation("Engellenen ödemeler sorgulanıyor");

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !c.IsDeleted (Global Query Filter)
        var checks = await _context.Set<PaymentFraudPrevention>()
            .AsNoTracking()
            .Include(c => c.Payment)
            .Where(c => c.IsBlocked)
            .OrderByDescending(c => c.RiskScore)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Engellenen ödemeler bulundu. Count: {Count}", checks.Count);

        return _mapper.Map<IEnumerable<PaymentFraudPreventionDto>>(checks);
    }
}
