using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Subscription;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Subscription.Queries.GetUserSubscriptionById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserSubscriptionByIdQueryHandler : IRequestHandler<GetUserSubscriptionByIdQuery, UserSubscriptionDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetUserSubscriptionByIdQueryHandler> _logger;

    public GetUserSubscriptionByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetUserSubscriptionByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserSubscriptionDto?> Handle(GetUserSubscriptionByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking for read-only query
        var subscription = await _context.Set<UserSubscription>()
            .AsNoTracking()
            .Include(us => us.User)
            .Include(us => us.SubscriptionPlan)
            .FirstOrDefaultAsync(us => us.Id == request.Id, cancellationToken);

        if (subscription == null)
        {
            _logger.LogWarning("User subscription not found. SubscriptionId: {SubscriptionId}", request.Id);
            return null;
        }

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        var dto = _mapper.Map<UserSubscriptionDto>(subscription);
        dto.DaysRemaining = subscription.EndDate > DateTime.UtcNow
            ? (int)(subscription.EndDate - DateTime.UtcNow).TotalDays
            : 0;

        // ✅ PERFORMANCE: Batch load recent payments
        var recentPayments = await _context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == subscription.Id)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .ToListAsync(cancellationToken);

        dto.RecentPayments = _mapper.Map<List<SubscriptionPaymentDto>>(recentPayments);

        return dto;
    }
}
