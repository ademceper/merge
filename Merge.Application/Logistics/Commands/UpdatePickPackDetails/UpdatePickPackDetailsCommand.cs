using MediatR;
using Merge.Domain.Enums;

namespace Merge.Application.Logistics.Commands.UpdatePickPackDetails;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Rich Domain Model - Status transition'ları için ayrı command'lar kullanılmalı (StartPicking, CompletePicking, StartPacking, CompletePacking, Ship, Cancel)
// Bu command sadece details (notes, weight, dimensions, packageCount) update için kullanılır
public record UpdatePickPackDetailsCommand(
    Guid PickPackId,
    string? Notes,
    decimal? Weight,
    string? Dimensions,
    int? PackageCount) : IRequest<Unit>;

