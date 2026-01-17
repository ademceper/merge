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

namespace Merge.Application.User.Queries.GetUserPreference;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUserPreferenceQueryHandler(IDbContext context, IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetUserPreferenceQueryHandler> logger) : IRequestHandler<GetUserPreferenceQuery, UserPreferenceDto>
{

    public async Task<UserPreferenceDto> Handle(GetUserPreferenceQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 9.2: Structured Logging (ZORUNLU)

        logger.LogInformation("Retrieving preferences for user: {UserId}", request.UserId);

        var preferences =         // ✅ PERFORMANCE: AsNoTracking
        await context.Set<UserPreference>()
            .AsNoTracking()
            .Where(up => up.UserId == request.UserId && !up.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (preferences == null)
        {
            logger.LogInformation("No preferences found for user: {UserId}, creating default preferences", request.UserId);

            preferences = UserPreference.Create(request.UserId);
            await context.Set<UserPreference>().AddAsync(preferences, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        
        // ✅ ARCHITECTURE: Domain event\'ler UnitOfWork.SaveChangesAsync içinde otomatik olarak OutboxMessage tablosuna yazılır

            logger.LogInformation("Default preferences created for user: {UserId}", request.UserId);
        }

                // ✅ ARCHITECTURE: AutoMapper kullan (manuel mapping YASAK)
        return mapper.Map<UserPreferenceDto>(preferences);
    }
}
