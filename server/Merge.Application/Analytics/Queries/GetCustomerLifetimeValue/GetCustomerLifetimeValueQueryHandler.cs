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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCustomerLifetimeValueQueryHandler(
    IDbContext context,
    ILogger<GetCustomerLifetimeValueQueryHandler> logger) : IRequestHandler<GetCustomerLifetimeValueQuery, CustomerLifetimeValueDto>
{

    public async Task<CustomerLifetimeValueDto> Handle(GetCustomerLifetimeValueQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching customer lifetime value. CustomerId: {CustomerId}", request.CustomerId);
        
        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        // ✅ PERFORMANCE: Removed manual !o.IsDeleted check (Global Query Filter handles it)
        var ltv = await context.Set<OrderEntity>()
            .AsNoTracking()
            .Where(o => o.UserId == request.CustomerId)
            .SumAsync(o => o.TotalAmount, cancellationToken);
        
        logger.LogInformation("Customer lifetime value calculated. CustomerId: {CustomerId}, LTV: {LifetimeValue}", request.CustomerId, ltv);

        // ✅ BOLUM 7.1: Records kullanımı - Constructor syntax
        return new CustomerLifetimeValueDto(request.CustomerId, ltv);
    }
}

