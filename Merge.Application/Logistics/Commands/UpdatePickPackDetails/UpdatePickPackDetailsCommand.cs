using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdatePickPackDetails;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
public record UpdatePickPackDetailsCommand(
    Guid PickPackId,
    PickPackStatus? Status,
    string? Notes,
    decimal? Weight,
    string? Dimensions,
    int? PackageCount) : IRequest<Unit>;

