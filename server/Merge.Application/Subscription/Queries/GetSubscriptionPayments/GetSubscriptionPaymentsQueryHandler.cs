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

public class GetSubscriptionPaymentsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetSubscriptionPaymentsQueryHandler> logger) : IRequestHandler<GetSubscriptionPaymentsQuery, IEnumerable<SubscriptionPaymentDto>>
{

    public async Task<IEnumerable<SubscriptionPaymentDto>> Handle(GetSubscriptionPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await context.Set<SubscriptionPayment>()
            .AsNoTracking()
            .Where(p => p.UserSubscriptionId == request.UserSubscriptionId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<SubscriptionPaymentDto>>(payments);
    }
}
