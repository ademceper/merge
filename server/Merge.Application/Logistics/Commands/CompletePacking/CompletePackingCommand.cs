using MediatR;

namespace Merge.Application.Logistics.Commands.CompletePacking;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CompletePackingCommand(
    Guid PickPackId,
    decimal Weight,
    string? Dimensions,
    int PackageCount) : IRequest<Unit>;

