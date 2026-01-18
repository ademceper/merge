using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Seller.Queries.GetStoreBySlug;

public record GetStoreBySlugQuery(
    string Slug
) : IRequest<StoreDto?>;
