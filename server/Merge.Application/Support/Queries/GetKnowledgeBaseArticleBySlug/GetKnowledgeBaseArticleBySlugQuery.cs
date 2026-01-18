using MediatR;
using Merge.Application.DTOs.Support;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Support.Queries.GetKnowledgeBaseArticleBySlug;

public record GetKnowledgeBaseArticleBySlugQuery(
    string Slug
) : IRequest<KnowledgeBaseArticleDto?>;
