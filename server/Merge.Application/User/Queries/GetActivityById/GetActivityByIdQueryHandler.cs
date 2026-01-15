using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Merge.Application.DTOs.User;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.User.Queries.GetActivityById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, UserActivityLogDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActivityByIdQueryHandler> _logger;

    public GetActivityByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetActivityByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserActivityLogDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        _logger.LogDebug("Retrieving activity with ID: {ActivityId}", request.Id);

        var activity =         // ✅ PERFORMANCE: AsNoTracking
        await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (activity == null)
        {
            _logger.LogWarning("Activity not found with ID: {ActivityId}", request.Id);
            return null;
        }

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return _mapper.Map<UserActivityLogDto>(activity);
    }
}
