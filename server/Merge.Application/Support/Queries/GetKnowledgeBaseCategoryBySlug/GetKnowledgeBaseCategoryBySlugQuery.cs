using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseCategoryBySlug;

public record GetKnowledgeBaseCategoryBySlugQuery(
    string Slug
) : IRequest<KnowledgeBaseCategoryDto?>;
