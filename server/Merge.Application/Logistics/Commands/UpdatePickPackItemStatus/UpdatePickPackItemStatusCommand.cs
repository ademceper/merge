using MediatR;

namespace Merge.Application.Logistics.Commands.UpdatePickPackItemStatus;

public record UpdatePickPackItemStatusCommand(
    Guid ItemId,
    bool? IsPicked,
    bool? IsPacked,
    string? Location) : IRequest<Unit>;

