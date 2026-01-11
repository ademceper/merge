using MediatR;
using Merge.Application.DTOs.Product;

namespace Merge.Application.Product.Commands.ImportProductsFromJson;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record ImportProductsFromJsonCommand(
    Stream FileStream
) : IRequest<BulkProductImportResultDto>;
