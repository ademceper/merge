using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetAvailableShippingProvidersQuery() : IRequest<IEnumerable<ShippingProviderDto>>;

