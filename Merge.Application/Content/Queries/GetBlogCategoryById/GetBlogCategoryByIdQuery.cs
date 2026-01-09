using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetBlogCategoryById;

public record GetBlogCategoryByIdQuery(Guid Id) : IRequest<BlogCategoryDto?>;

