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
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.User.Queries.GetActivityById;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, UserActivityLogDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<GetActivityByIdQueryHandler> _logger;

    public GetActivityByIdQueryHandler(
        IDbContext context,
        IMapper mapper,
        ILogger<GetActivityByIdQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<UserActivityLogDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving activity with ID: {ActivityId}", request.Id);

        var activity = await _context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (activity == null)
        {
            _logger.LogWarning("Activity not found with ID: {ActivityId}", request.Id);
            return null;
        }

        return _mapper.Map<UserActivityLogDto>(activity);
    }
}
