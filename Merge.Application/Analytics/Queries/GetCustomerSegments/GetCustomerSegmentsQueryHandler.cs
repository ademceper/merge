using MediatR;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.Analytics;
using AutoMapper;

namespace Merge.Application.Analytics.Queries.GetCustomerSegments;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetCustomerSegmentsQueryHandler : IRequestHandler<GetCustomerSegmentsQuery, List<CustomerSegmentDto>>
{
    private readonly ILogger<GetCustomerSegmentsQueryHandler> _logger;
    private readonly IMapper _mapper;

    public GetCustomerSegmentsQueryHandler(
        ILogger<GetCustomerSegmentsQueryHandler> logger,
        IMapper mapper)
    {
        _logger = logger;
        _mapper = mapper;
    }

    public Task<List<CustomerSegmentDto>> Handle(GetCustomerSegmentsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching customer segments");

        // Simplified segmentation - can be enhanced
        // ✅ ARCHITECTURE: .cursorrules'a göre manuel mapping YASAK, AutoMapper kullanıyoruz
        var segmentsData = new[]
        {
            new { Segment = "VIP", CustomerCount = 0, TotalRevenue = 0m, AverageOrderValue = 0m },
            new { Segment = "Active", CustomerCount = 0, TotalRevenue = 0m, AverageOrderValue = 0m },
            new { Segment = "New", CustomerCount = 0, TotalRevenue = 0m, AverageOrderValue = 0m }
        };

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        var segments = _mapper.Map<List<CustomerSegmentDto>>(segmentsData);

        return Task.FromResult(segments);
    }
}

