using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetAdminTopProducts;

public record GetAdminTopProductsQuery(
    int Count
) : IRequest<IEnumerable<AdminTopProductDto>>;

