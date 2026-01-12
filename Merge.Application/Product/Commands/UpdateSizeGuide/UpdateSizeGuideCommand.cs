using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UpdateSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record UpdateSizeGuideCommand(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId,
    string? Brand,
    string Type,
    string MeasurementUnit,
    List<CreateSizeGuideEntryDto> Entries
) : IRequest<bool>;
