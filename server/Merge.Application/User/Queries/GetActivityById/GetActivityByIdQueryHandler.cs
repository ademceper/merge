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

public class GetActivityByIdQueryHandler(IDbContext context, IMapper mapper, ILogger<GetActivityByIdQueryHandler> logger) : IRequestHandler<GetActivityByIdQuery, UserActivityLogDto?>
{

    public async Task<UserActivityLogDto?> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {

        logger.LogDebug("Retrieving activity with ID: {ActivityId}", request.Id);

        var activity =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<UserActivityLog>()
            .AsNoTracking()
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (activity is null)
        {
            logger.LogWarning("Activity not found with ID: {ActivityId}", request.Id);
            return null;
        }

        return mapper.Map<UserActivityLogDto>(activity);
    }
}
