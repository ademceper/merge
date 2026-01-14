using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToExcel;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class ExportProductsToExcelQueryHandler : IRequestHandler<ExportProductsToExcelQuery, byte[]>
{
    private readonly IMediator _mediator;
    private readonly ILogger<ExportProductsToExcelQueryHandler> _logger;

    public ExportProductsToExcelQueryHandler(
        IMediator mediator,
        ILogger<ExportProductsToExcelQueryHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<byte[]> Handle(ExportProductsToExcelQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Exporting products to Excel. CategoryId: {CategoryId}, ActiveOnly: {ActiveOnly}",
            request.ExportDto.CategoryId, request.ExportDto.ActiveOnly);

        // For Excel export, we'll use CSV format as a simple alternative
        // In production, use EPPlus or ClosedXML library for real Excel files
        var csvQuery = new Queries.ExportProductsToCsv.ExportProductsToCsvQuery(request.ExportDto);
        var result = await _mediator.Send(csvQuery, cancellationToken);

        _logger.LogInformation("Excel export completed (CSV format).");

        return result;
    }
}
