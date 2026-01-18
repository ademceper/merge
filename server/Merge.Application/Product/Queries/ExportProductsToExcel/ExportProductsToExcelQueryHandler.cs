using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.ExportProductsToExcel;

public class ExportProductsToExcelQueryHandler(IMediator mediator, ILogger<ExportProductsToExcelQueryHandler> logger) : IRequestHandler<ExportProductsToExcelQuery, byte[]>
{

    public async Task<byte[]> Handle(ExportProductsToExcelQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Exporting products to Excel. CategoryId: {CategoryId}, ActiveOnly: {ActiveOnly}",
            request.ExportDto.CategoryId, request.ExportDto.ActiveOnly);

        // For Excel export, we'll use CSV format as a simple alternative
        // In production, use EPPlus or ClosedXML library for real Excel files
        var csvQuery = new Queries.ExportProductsToCsv.ExportProductsToCsvQuery(request.ExportDto);
        var result = await mediator.Send(csvQuery, cancellationToken);

        logger.LogInformation("Excel export completed (CSV format).");

        return result;
    }
}
