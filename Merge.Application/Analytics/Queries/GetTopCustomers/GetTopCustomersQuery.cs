using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetTopCustomers;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetTopCustomersQuery(
    int Limit
) : IRequest<List<TopCustomerDto>>;

