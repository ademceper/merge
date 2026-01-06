using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerSegments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public record GetCustomerSegmentsQuery() : IRequest<List<CustomerSegmentDto>>;

