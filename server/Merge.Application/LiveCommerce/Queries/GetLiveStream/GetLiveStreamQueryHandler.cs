using MediatR;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.LiveCommerce;
using Merge.Application.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.LiveCommerce.Queries.GetLiveStream;

public class GetLiveStreamQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetLiveStreamQueryHandler> logger) : IRequestHandler<GetLiveStreamQuery, LiveStreamDto?>
{
    public async Task<LiveStreamDto?> Handle(GetLiveStreamQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Getting live stream. StreamId: {StreamId}", request.Id);

        var stream = await context.Set<LiveStream>()
            .AsNoTracking()
            .Include(s => s.Seller)
            .Include(s => s.Products)
                .ThenInclude(p => p.Product)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return stream != null ? mapper.Map<LiveStreamDto>(stream) : null;
    }
}
