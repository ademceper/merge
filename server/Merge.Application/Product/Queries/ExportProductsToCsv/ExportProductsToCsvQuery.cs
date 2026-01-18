using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToCsv;

public record ExportProductsToCsvQuery(
    BulkProductExportDto ExportDto
) : IRequest<byte[]>;
