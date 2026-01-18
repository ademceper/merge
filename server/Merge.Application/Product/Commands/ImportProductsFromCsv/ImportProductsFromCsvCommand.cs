using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ImportProductsFromCsv;

public record ImportProductsFromCsvCommand(
    Stream FileStream
) : IRequest<BulkProductImportResultDto>;
