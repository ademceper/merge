using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Application.Product.Commands.ImportProductsFromCsv;
using Merge.Application.Product.Commands.ImportProductsFromJson;
using Merge.Application.Product.Queries.ExportProductsToCsv;
using Merge.Application.Product.Queries.ExportProductsToJson;
using Merge.Application.Product.Queries.ExportProductsToExcel;
using Merge.API.Middleware;
using Merge.API.Helpers;

namespace Merge.API.Controllers.Product;

/// <summary>
/// Bulk Products API endpoints.
/// Toplu ürün işlemlerini yönetir.
/// </summary>
[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/bulk")]
[Authorize(Roles = "Admin")]
[Tags("BulkProducts")]
public class BulkProductsController(IMediator mediator) : BaseController
{
    /// <summary>
    /// CSV dosyasından ürün içe aktarır
    /// </summary>
    /// <param name="file">CSV dosyası</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İçe aktarma sonucu</returns>
    /// <response code="200">İçe aktarma başarılı</response>
    /// <response code="400">Geçersiz dosya veya format</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("import/csv")]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(BulkProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BulkProductImportResultDto>> ImportFromCsv(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Problem("File is required and cannot be empty", "Validation Error", StatusCodes.Status400BadRequest);
        }
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Problem("File must be a CSV file", "Validation Error", StatusCodes.Status400BadRequest);
        }

        using var stream = file.OpenReadStream();
        var command = new ImportProductsFromCsvCommand(stream);
        var result = await mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// JSON dosyasından ürün içe aktarır
    /// </summary>
    /// <param name="file">JSON dosyası</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İçe aktarma sonucu</returns>
    /// <response code="200">İçe aktarma başarılı</response>
    /// <response code="400">Geçersiz dosya veya format</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("import/json")]
    [RateLimit(5, 3600)]
    [ProducesResponseType(typeof(BulkProductImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<BulkProductImportResultDto>> ImportFromJson(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Problem("File is required and cannot be empty", "Validation Error", StatusCodes.Status400BadRequest);
        }
        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return Problem("File must be a JSON file", "Validation Error", StatusCodes.Status400BadRequest);
        }

        using var stream = file.OpenReadStream();
        var command = new ImportProductsFromJsonCommand(stream);
        var result = await mediator.Send(command, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Ürünleri CSV formatında dışa aktarır
    /// </summary>
    /// <param name="exportDto">Dışa aktarma parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>CSV dosyası</returns>
    /// <response code="200">Dışa aktarma başarılı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("export/csv")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToCsv(
        [FromBody] BulkProductExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportProductsToCsvQuery(exportDto);
        var csvData = await mediator.Send(query, cancellationToken);
        
        return File(csvData, "text/csv", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

    /// <summary>
    /// Ürünleri JSON formatında dışa aktarır
    /// </summary>
    /// <param name="exportDto">Dışa aktarma parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>JSON dosyası</returns>
    /// <response code="200">Dışa aktarma başarılı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("export/json")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToJson(
        [FromBody] BulkProductExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportProductsToJsonQuery(exportDto);
        var jsonData = await mediator.Send(query, cancellationToken);
        
        return File(jsonData, "application/json", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

    /// <summary>
    /// Ürünleri Excel formatında dışa aktarır
    /// </summary>
    /// <param name="exportDto">Dışa aktarma parametreleri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Excel dosyası</returns>
    /// <response code="200">Dışa aktarma başarılı</response>
    /// <response code="400">Geçersiz parametreler</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpPost("export/excel")]
    [RateLimit(10, 60)]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ExportToExcel(
        [FromBody] BulkProductExportDto exportDto,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportProductsToExcelQuery(exportDto);
        var excelData = await mediator.Send(query, cancellationToken);
        
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    /// <summary>
    /// CSV içe aktarma şablonunu indirir
    /// </summary>
    /// <returns>CSV şablon dosyası</returns>
    /// <response code="200">Şablon başarıyla indirildi</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("template/csv")]
    [RateLimit(60, 60)]
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

    /// <summary>
    /// JSON içe aktarma şablonunu indirir
    /// </summary>
    /// <returns>JSON şablon dosyası</returns>
    /// <response code="200">Şablon başarıyla indirildi</response>
    /// <response code="401">Kimlik doğrulama gerekli</response>
    /// <response code="403">Yetki yok</response>
    /// <response code="429">Rate limit aşıldı</response>
    [HttpGet("template/json")]
    [RateLimit(60, 60)]
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
    ""ImageUrl"": ""https:
    ""IsActive"": true
  }
]";
        var bytes = System.Text.Encoding.UTF8.GetBytes(template);
        
        return File(bytes, "application/json", "product_import_template.json");
    }
}
