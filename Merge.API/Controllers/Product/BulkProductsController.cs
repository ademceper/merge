using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;
using Merge.API.Middleware;

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
namespace Merge.API.Controllers.Product;

[ApiController]
[Route("api/products/bulk")]
[Authorize(Roles = "Admin")]
public class BulkProductsController : BaseController
{
    private readonly IBulkProductService _bulkProductService;

    public BulkProductsController(IBulkProductService bulkProductService)
    {
        _bulkProductService = bulkProductService;
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("import/csv")]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5/saat (Bulk import is expensive)
    [ProducesResponseType(typeof(BulkProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BulkProductImportResultDto>> ImportFromCsv(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (file == null || file.Length == 0)
        {
            return BadRequest();
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        using var stream = file.OpenReadStream();
        var result = await _bulkProductService.ImportProductsFromCsvAsync(stream, cancellationToken);
        return Ok(result);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("import/json")]
    [RateLimit(5, 3600)] // ✅ BOLUM 3.3: Rate Limiting - 5/saat (Bulk import is expensive)
    [ProducesResponseType(typeof(BulkProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BulkProductImportResultDto>> ImportFromJson(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        if (file == null || file.Length == 0)
        {
            return BadRequest();
        }

        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        using var stream = file.OpenReadStream();
        var result = await _bulkProductService.ImportProductsFromJsonAsync(stream, cancellationToken);
        return Ok(result);
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("export/csv")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToCsv(
        [FromBody] BulkProductExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var csvData = await _bulkProductService.ExportProductsToCsvAsync(exportDto, cancellationToken);
        return File(csvData, "text/csv", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("export/json")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToJson(
        [FromBody] BulkProductExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var jsonData = await _bulkProductService.ExportProductsToJsonAsync(exportDto, cancellationToken);
        return File(jsonData, "application/json", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpPost("export/excel")]
    [RateLimit(10, 60)] // ✅ BOLUM 3.3: Rate Limiting - 10/dakika
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToExcel(
        [FromBody] BulkProductExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        // ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
        var excelData = await _bulkProductService.ExportProductsToExcelAsync(exportDto, cancellationToken);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("template/csv")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult DownloadCsvTemplate()
    {
        var template = "Name,Description,SKU,Price,DiscountPrice,StockQuantity,Brand,Category,ImageUrl\n" +
                      "\"Sample Product\",\"Product description\",\"SKU001\",99.99,79.99,100,\"Brand Name\",\"Electronics\",\"https://example.com/image.jpg\"";

        var bytes = System.Text.Encoding.UTF8.GetBytes(template);
        return File(bytes, "text/csv", "product_import_template.csv");
    }

    // ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
    // ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
    [HttpGet("template/json")]
    [RateLimit(60, 60)] // ✅ BOLUM 3.3: Rate Limiting - 60/dakika (DoS koruması)
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult DownloadJsonTemplate()
    {
        var template = @"[
  {
    ""Name"": ""Sample Product"",
    ""Description"": ""Product description"",
    ""SKU"": ""SKU001"",
    ""Price"": 99.99,
    ""DiscountPrice"": 79.99,
    ""StockQuantity"": 100,
    ""Brand"": ""Brand Name"",
    ""CategoryName"": ""Electronics"",
    ""ImageUrl"": ""https://example.com/image.jpg"",
    ""IsActive"": true
  }
]";

        var bytes = System.Text.Encoding.UTF8.GetBytes(template);
        return File(bytes, "application/json", "product_import_template.json");
    }
}
