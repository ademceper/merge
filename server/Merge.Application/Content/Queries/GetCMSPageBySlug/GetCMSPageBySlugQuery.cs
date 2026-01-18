using MediatR;
using Merge.Application.DTOs.Content;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Content.Queries.GetCMSPageBySlug;

public record GetCMSPageBySlugQuery(
    string Slug
) : IRequest<CMSPageDto?>;

