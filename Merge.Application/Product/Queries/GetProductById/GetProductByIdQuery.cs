using MediatR;
using Merge.Application.DTOs.Product;
using Merge.Domain.Modules.Catalog;

namespace Merge.Application.Product.Queries.GetProductById;

// BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetProductByIdQuery(
    Guid ProductId
) : IRequest<ProductDto?>;
