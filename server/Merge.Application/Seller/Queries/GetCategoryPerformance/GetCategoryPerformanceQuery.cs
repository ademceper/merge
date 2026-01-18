using MediatR;
using Merge.Application.DTOs.Seller;

namespace Merge.Application.Seller.Queries.GetCategoryPerformance;

public record GetCategoryPerformanceQuery(
    Guid SellerId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IRequest<List<CategoryPerformanceDto>>;
