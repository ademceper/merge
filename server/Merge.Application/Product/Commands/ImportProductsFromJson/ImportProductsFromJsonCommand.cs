using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Commands.ImportProductsFromJson;

public record ImportProductsFromJsonCommand(
    Stream FileStream
) : IRequest<BulkProductImportResultDto>;
