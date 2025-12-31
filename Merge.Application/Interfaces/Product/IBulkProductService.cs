using Merge.Application.DTOs.Product;
namespace Merge.Application.Interfaces.Product;

public interface IBulkProductService
{
    Task<BulkProductImportResultDto> ImportProductsFromCsvAsync(Stream fileStream);
    Task<BulkProductImportResultDto> ImportProductsFromJsonAsync(Stream fileStream);
    Task<byte[]> ExportProductsToCsvAsync(BulkProductExportDto exportDto);
    Task<byte[]> ExportProductsToJsonAsync(BulkProductExportDto exportDto);
    Task<byte[]> ExportProductsToExcelAsync(BulkProductExportDto exportDto);
}
