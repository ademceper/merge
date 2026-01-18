using MediatR;
using Merge.Application.DTOs.Logistics;

namespace Merge.Application.Logistics.Queries.GetAvailableShippingProviders;

public record GetAvailableShippingProvidersQuery() : IRequest<IEnumerable<ShippingProviderDto>>;

