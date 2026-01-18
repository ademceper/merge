using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Commands.OptimizePrice;

public record OptimizePriceCommand(
    Guid ProductId,
    PriceOptimizationRequestDto? Request = null) : IRequest<PriceOptimizationDto>;
