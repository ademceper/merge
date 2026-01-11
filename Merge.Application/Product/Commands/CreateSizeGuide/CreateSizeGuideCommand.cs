using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.CreateSizeGuide;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record CreateSizeGuideCommand(
    string Name,
    string Description,
    Guid CategoryId,
    string? Brand,
    string Type,
    string MeasurementUnit,
    List<CreateSizeGuideEntryDto> Entries
) : IRequest<SizeGuideDto>;
