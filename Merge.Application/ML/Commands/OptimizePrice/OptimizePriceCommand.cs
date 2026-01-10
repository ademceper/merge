using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.ML.Commands.OptimizePrice;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record OptimizePriceCommand(
    Guid ProductId,
    PriceOptimizationRequestDto? Request = null) : IRequest<PriceOptimizationDto>;
