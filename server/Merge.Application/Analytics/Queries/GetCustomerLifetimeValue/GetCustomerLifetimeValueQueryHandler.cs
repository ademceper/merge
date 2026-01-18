using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using OrderEntity = Merge.Domain.Modules.Ordering.Order;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;

public class GetCustomerLifetimeValueQueryHandler(
    IDbContext context,
    ILogger<GetCustomerLifetimeValueQueryHandler> logger) : IRequestHandler<GetCustomerLifetimeValueQuery, CustomerLifetimeValueDto>
{

    public async Task<CustomerLifetimeValueDto> Handle(GetCustomerLifetimeValueQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching customer lifetime value. CustomerId: {CustomerId}", request.CustomerId);
        
        var ltv = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.UserId == request.CustomerId)
            .SumAsync(o => o.TotalAmount, cancellationToken);
        
        logger.LogInformation("Customer lifetime value calculated. CustomerId: {CustomerId}, LTV: {LifetimeValue}", request.CustomerId, ltv);

        return new CustomerLifetimeValueDto(request.CustomerId, ltv);
    }
}

