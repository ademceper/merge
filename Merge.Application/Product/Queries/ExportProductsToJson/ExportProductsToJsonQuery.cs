using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.ExportProductsToJson;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ExportProductsToJsonQuery(
    BulkProductExportDto ExportDto
) : IRequest<byte[]>;
