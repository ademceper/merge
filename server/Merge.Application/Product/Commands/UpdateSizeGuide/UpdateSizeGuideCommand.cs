using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.UpdateSizeGuide;

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
