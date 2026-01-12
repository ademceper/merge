using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.LiveCommerce.Queries.GetLiveStream;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor
public class GetLiveStreamQueryHandler : IRequestHandler<GetLiveStreamQuery, LiveStreamDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetLiveStreamQueryHandler> _logger;

    public GetLiveStreamQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetLiveStreamQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LiveStreamDto?> Handle(GetLiveStreamQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting live stream. StreamId: {StreamId}", request.Id);

        // ✅ PERFORMANCE: AsNoTracking (read-only query)
        // ✅ PERFORMANCE: Include ile N+1 önlenir
        var stream = await _context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return stream != null ? _mapper.Map<LiveStreamDto>(stream) : null;
    }
}

