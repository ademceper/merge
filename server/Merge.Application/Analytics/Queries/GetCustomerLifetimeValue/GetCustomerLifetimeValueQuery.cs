using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerLifetimeValue;

public record GetCustomerLifetimeValueQuery(
    Guid CustomerId
) : IRequest<CustomerLifetimeValueDto>;

