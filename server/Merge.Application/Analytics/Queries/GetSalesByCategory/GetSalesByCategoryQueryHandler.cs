using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Analytics.Queries.GetSalesByCategory;

public class GetSalesByCategoryQueryHandler(
    IDbContext context,
    ILogger<GetSalesByCategoryQueryHandler> logger) : IRequestHandler<GetSalesByCategoryQuery, List<CategorySalesDto>>
{

    public async Task<List<CategorySalesDto>> Handle(GetSalesByCategoryQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching sales by category. StartDate: {StartDate}, EndDate: {EndDate}",
            request.StartDate, request.EndDate);

        return await context.Set<OrderItem>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(oi => oi.Product)
            .ThenInclude(p => p.Category)
            .Include(oi => oi.Order)
            .Where(oi => oi.Order.CreatedAt >= request.StartDate && oi.Order.CreatedAt <= request.EndDate && oi.Product.Category != null)
            .GroupBy(oi => new { oi.Product.CategoryId, CategoryName = oi.Product.Category!.Name })
            .Select(g => new CategorySalesDto(
                g.Key.CategoryId,
                g.Key.CategoryName,
                g.Sum(oi => oi.TotalPrice),
                g.Select(oi => oi.OrderId).Distinct().Count(),
                g.Sum(oi => oi.Quantity)
            ))
            .OrderByDescending(c => c.Revenue)
            .ToListAsync(cancellationToken);
    }
}

