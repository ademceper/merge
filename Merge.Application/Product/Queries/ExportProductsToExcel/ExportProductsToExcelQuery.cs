using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Queries.ExportProductsToExcel;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ExportProductsToExcelQuery(
    BulkProductExportDto ExportDto
) : IRequest<byte[]>;
