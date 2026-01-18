using MediatR;

namespace Merge.Application.Logistics.Commands.CompletePacking;

public record CompletePackingCommand(
    Guid PickPackId,
    decimal Weight,
    string? Dimensions,
    int PackageCount) : IRequest<Unit>;

