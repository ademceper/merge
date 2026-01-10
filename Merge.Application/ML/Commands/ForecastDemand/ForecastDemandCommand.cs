using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Commands.ForecastDemand;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ForecastDemandCommand(
    Guid ProductId,
    int ForecastDays = 30) : IRequest<DemandForecastDto>;
