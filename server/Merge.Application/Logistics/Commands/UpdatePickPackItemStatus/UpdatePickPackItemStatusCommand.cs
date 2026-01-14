using MediatR;

namespace Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdatePickPackItemStatusCommand(
    Guid ItemId,
    bool? IsPicked,
    bool? IsPacked,
    string? Location) : IRequest<Unit>;

