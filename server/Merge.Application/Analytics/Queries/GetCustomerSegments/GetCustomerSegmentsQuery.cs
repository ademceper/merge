using MediatR;
using Merge.Application.DTOs.Analytics;

namespace Merge.Application.Analytics.Queries.GetCustomerSegments;

public record GetCustomerSegmentsQuery() : IRequest<List<CustomerSegmentDto>>;

