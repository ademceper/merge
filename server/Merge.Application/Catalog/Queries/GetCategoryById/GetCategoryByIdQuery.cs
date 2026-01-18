using MediatR;
using Merge.Application.DTOs.Catalog;

namespace Merge.Application.Catalog.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

