using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetInventoryOverview;

public record GetInventoryOverviewQuery() : IRequest<InventoryOverviewDto>;

