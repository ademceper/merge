using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetQAStats;

public record GetQAStatsQuery(
    Guid? ProductId = null
) : IRequest<QAStatsDto>;
