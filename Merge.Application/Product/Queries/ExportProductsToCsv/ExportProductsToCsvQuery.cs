using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.ExportProductsToCsv;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ExportProductsToCsvQuery(
    BulkProductExportDto ExportDto
) : IRequest<byte[]>;
