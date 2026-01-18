using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToExcel;

public record ExportProductsToExcelQuery(
    BulkProductExportDto ExportDto
) : IRequest<byte[]>;
