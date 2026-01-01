using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Merge.Application.Interfaces.User;
using Merge.Application.Interfaces.Product;
using Merge.Application.DTOs.Product;

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

    [HttpPost("import/csv")]
    [ProducesResponseType(typeof(BulkProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BulkProductImportResultDto>> ImportFromCsv(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest();
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        using var stream = file.OpenReadStream();
        var result = await _bulkProductService.ImportProductsFromCsvAsync(stream);
        return Ok(result);
    }

    [HttpPost("import/json")]
    [ProducesResponseType(typeof(BulkProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BulkProductImportResultDto>> ImportFromJson(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest();
        }

        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        using var stream = file.OpenReadStream();
        var result = await _bulkProductService.ImportProductsFromJsonAsync(stream);
        return Ok(result);
    }

    [HttpPost("export/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToCsv([FromBody] BulkProductExportDto exportDto)
    {
        var csvData = await _bulkProductService.ExportProductsToCsvAsync(exportDto);
        return File(csvData, "text/csv", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    [HttpPost("export/json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToJson([FromBody] BulkProductExportDto exportDto)
    {
        var jsonData = await _bulkProductService.ExportProductsToJsonAsync(exportDto);
        return File(jsonData, "application/json", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    [HttpPost("export/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportToExcel([FromBody] BulkProductExportDto exportDto)
    {
        var excelData = await _bulkProductService.ExportProductsToExcelAsync(exportDto);
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    [HttpGet("template/csv")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult DownloadCsvTemplate()
    {
        var template = "Name,Description,SKU,Price,DiscountPrice,StockQuantity,Brand,Category,ImageUrl\n" +
                      "\"Sample Product\",\"Product description\",\"SKU001\",99.99,79.99,100,\"Brand Name\",\"Electronics\",\"https://example.com/image.jpg\"";

        var bytes = System.Text.Encoding.UTF8.GetBytes(template);
        return File(bytes, "text/csv", "product_import_template.csv");
    }

    [HttpGet("template/json")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
