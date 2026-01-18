using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToJson;

public record ExportProductsToJsonQuery(
    BulkProductExportDto ExportDto
) : IRequest<byte[]>;
