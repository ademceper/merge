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

// ✅ BOLUM 2.2: CancellationToken destegi (ZORUNLU)
// ✅ BOLUM 3.1: ProducesResponseType (ZORUNLU)
// ✅ BOLUM 3.3: Rate Limiting (ZORUNLU)
// ✅ BOLUM 4.0: API Versioning (ZORUNLU)
// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
namespace Merge.API.Controllers.Product;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/bulk")]
[Authorize(Roles = "Admin")]
public class BulkProductsController : BaseController
{
    private readonly IMediator _mediator;

    public BulkProductsController(IMediator mediator)
    {
        _mediator = mediator;
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        using var stream = file.OpenReadStream();
        var command = new ImportProductsFromCsvCommand(stream);
        var result = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/import/csv",
                Method = "POST"
            },
            ["exportCsv"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/export/csv",
                Method = "POST"
            },
            ["exportJson"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/export/json",
                Method = "POST"
            },
            ["exportExcel"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/export/excel",
                Method = "POST"
            },
            ["downloadCsvTemplate"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/template/csv",
                Method = "GET"
            },
            ["downloadJsonTemplate"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/template/json",
                Method = "GET"
            }
        };
        
        return Ok(new { data = result, _links = links });
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

        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        using var stream = file.OpenReadStream();
        var command = new ImportProductsFromJsonCommand(stream);
        var result = await _mediator.Send(command, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/import/json",
                Method = "POST"
            },
            ["exportCsv"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/export/csv",
                Method = "POST"
            },
            ["exportJson"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/export/json",
                Method = "POST"
            },
            ["exportExcel"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/export/excel",
                Method = "POST"
            },
            ["downloadCsvTemplate"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/template/csv",
                Method = "GET"
            },
            ["downloadJsonTemplate"] = new LinkDto
            {
                Href = $"/api/v{version}/products/bulk/template/json",
                Method = "GET"
            }
        };
        
        return Ok(new { data = result, _links = links });
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new ExportProductsToCsvQuery(exportDto);
        var csvData = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        // Note: File responses don't support HATEOAS links in the body, but we can add them in headers
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        Response.Headers.Add("Link", $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/csv>; rel=\"import-csv\", " +
                                     $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/json>; rel=\"import-json\"");
        
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new ExportProductsToJsonQuery(exportDto);
        var jsonData = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        // Note: File responses don't support HATEOAS links in the body, but we can add them in headers
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        Response.Headers.Add("Link", $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/csv>; rel=\"import-csv\", " +
                                     $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/json>; rel=\"import-json\"");
        
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
        // ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
        var query = new ExportProductsToExcelQuery(exportDto);
        var excelData = await _mediator.Send(query, cancellationToken);
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        // Note: File responses don't support HATEOAS links in the body, but we can add them in headers
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        Response.Headers.Add("Link", $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/csv>; rel=\"import-csv\", " +
                                     $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/json>; rel=\"import-json\"");
        
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
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        // Note: File responses don't support HATEOAS links in the body, but we can add them in headers
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        Response.Headers.Add("Link", $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/csv>; rel=\"import-csv\", " +
                                     $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/json>; rel=\"import-json\"");
        
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
        
        // ✅ BOLUM 4.1.3: HATEOAS - Hypermedia links (ZORUNLU)
        // Note: File responses don't support HATEOAS links in the body, but we can add them in headers
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        Response.Headers.Add("Link", $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/csv>; rel=\"import-csv\", " +
                                     $"<{Request.Scheme}://{Request.Host}/api/v{version}/products/bulk/import/json>; rel=\"import-json\"");
        
        return File(bytes, "application/json", "product_import_template.json");
    }
}
