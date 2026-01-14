using MediatR;
using Merge.Application.DTOs.Seller;
using Merge.Domain.ValueObjects;

namespace Merge.Application.Seller.Queries.GetStoreBySlug;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetStoreBySlugQuery(
    string Slug
) : IRequest<StoreDto?>;
