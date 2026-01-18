using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetSystemHealth;

public record GetSystemHealthQuery() : IRequest<SystemHealthDto>;

