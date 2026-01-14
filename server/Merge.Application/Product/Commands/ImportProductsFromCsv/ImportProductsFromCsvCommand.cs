using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ImportProductsFromCsv;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ImportProductsFromCsvCommand(
    Stream FileStream
) : IRequest<BulkProductImportResultDto>;
