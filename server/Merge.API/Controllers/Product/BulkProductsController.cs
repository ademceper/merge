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

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/products/bulk")]
[Authorize(Roles = "Admin")]
public class BulkProductsController(IMediator mediator) : BaseController
{
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
            return BadRequest();
        }
        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        using var stream = file.OpenReadStream();
        var command = new ImportProductsFromCsvCommand(stream);
        var result = await mediator.Send(command, cancellationToken);
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
            return BadRequest();
        }
        if (!file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        using var stream = file.OpenReadStream();
        var command = new ImportProductsFromJsonCommand(stream);
        var result = await mediator.Send(command, cancellationToken);
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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        
        return File(csvData, "text/csv", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }

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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        
        return File(jsonData, "application/json", $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json");
    }

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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        
        return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"products_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        
        return File(bytes, "text/csv", "product_import_template.csv");
    }

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
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0";
        
        return File(bytes, "application/json", "product_import_template.json");
    }
}
