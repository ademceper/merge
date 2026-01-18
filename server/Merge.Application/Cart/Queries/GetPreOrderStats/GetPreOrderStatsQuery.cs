using MediatR;
using Merge.Application.DTOs.Cart;
using Merge.Domain.Modules.Ordering;

namespace Merge.Application.Cart.Queries.GetPreOrderStats;

public record GetPreOrderStatsQuery : IRequest<PreOrderStatsDto>;

