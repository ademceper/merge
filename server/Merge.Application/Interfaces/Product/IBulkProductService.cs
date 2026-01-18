using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Interfaces.Product;

public interface IBulkProductService
{
    Task<BulkProductImportResultDto> ImportProductsFromCsvAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<BulkProductImportResultDto> ImportProductsFromJsonAsync(Stream fileStream, CancellationToken cancellationToken = default);
    Task<byte[]> ExportProductsToCsvAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default);
    Task<byte[]> ExportProductsToJsonAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default);
    Task<byte[]> ExportProductsToExcelAsync(BulkProductExportDto exportDto, CancellationToken cancellationToken = default);
}
