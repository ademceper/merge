using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetSystemHealth;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetSystemHealthQuery() : IRequest<SystemHealthDto>;

