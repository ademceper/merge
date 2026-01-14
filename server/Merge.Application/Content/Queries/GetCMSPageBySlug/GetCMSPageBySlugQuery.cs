using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetCMSPageBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCMSPageBySlugQuery(
    string Slug
) : IRequest<CMSPageDto?>;

