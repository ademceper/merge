using MediatR;
using Merge.Application.DTOs.Cart;

namespace Merge.Application.Cart.Queries.GetPreOrderStats;

public record GetPreOrderStatsQuery : IRequest<PreOrderStatsDto>;

