using MediatR;
using Merge.Application.DTOs.Support;

namespace Merge.Application.Support.Queries.GetTicketStats;

public record GetTicketStatsQuery() : IRequest<TicketStatsDto>;
