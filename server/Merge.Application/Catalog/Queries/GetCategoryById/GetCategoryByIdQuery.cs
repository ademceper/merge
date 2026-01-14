using MediatR;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Queries.GetCategoryById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

