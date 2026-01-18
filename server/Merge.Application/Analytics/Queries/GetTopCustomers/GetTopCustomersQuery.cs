using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

public record GetTopCustomersQuery(
    int Limit
) : IRequest<List<TopCustomerDto>>;

