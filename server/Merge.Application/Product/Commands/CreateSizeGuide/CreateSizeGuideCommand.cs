using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.CreateSizeGuide;

public record CreateSizeGuideCommand(
    string Name,
    string Description,
    Guid CategoryId,
    string? Brand,
    string Type,
    string MeasurementUnit,
    List<CreateSizeGuideEntryDto> Entries
) : IRequest<SizeGuideDto>;
