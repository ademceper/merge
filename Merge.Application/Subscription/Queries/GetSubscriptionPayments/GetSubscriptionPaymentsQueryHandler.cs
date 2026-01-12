using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Subscription.Queries.GetSubscriptionPayments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetSubscriptionPaymentsQueryHandler : IRequestHandler<GetSubscriptionPaymentsQuery, IEnumerable<SubscriptionPaymentDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetSubscriptionPaymentsQueryHandler> _logger;

    public GetSubscriptionPaymentsQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetSubscriptionPaymentsQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<SubscriptionPaymentDto>> Handle(GetSubscriptionPaymentsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var payments = await _context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == request.UserSubscriptionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<SubscriptionPaymentDto>>(payments);
    }
}
