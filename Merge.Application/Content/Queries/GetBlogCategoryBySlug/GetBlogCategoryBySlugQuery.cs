using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetBlogCategoryBySlug;

public record GetBlogCategoryBySlugQuery(string Slug) : IRequest<BlogCategoryDto?>;

