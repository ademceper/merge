using MediatR;
using Merge.Application.DTOs.Content;

namespace Merge.Application.Content.Queries.GetBlogCategoryBySlug;

public record GetBlogCategoryBySlugQuery(string Slug) : IRequest<BlogCategoryDto?>;

