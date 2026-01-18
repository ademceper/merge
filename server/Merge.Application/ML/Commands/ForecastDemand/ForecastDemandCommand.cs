using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Commands.ForecastDemand;

public record ForecastDemandCommand(
    Guid ProductId,
    int ForecastDays = 30) : IRequest<DemandForecastDto>;
