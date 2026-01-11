using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Review.Queries.GetReviewMedia;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetReviewMediaQueryHandler : IRequestHandler<GetReviewMediaQuery, IEnumerable<ReviewMediaDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetReviewMediaQueryHandler> _logger;

    public GetReviewMediaQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetReviewMediaQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ReviewMediaDto>> Handle(GetReviewMediaQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching review media. ReviewId: {ReviewId}", request.ReviewId);

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        var media = await _context.Set<ReviewMedia>()
            .AsNoTracking()
            .Where(m => m.ReviewId == request.ReviewId)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<IEnumerable<ReviewMediaDto>>(media);
    }
}
