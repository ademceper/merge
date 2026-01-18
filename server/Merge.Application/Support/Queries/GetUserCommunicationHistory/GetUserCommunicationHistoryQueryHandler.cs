using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetUserCommunicationHistory;

public class GetUserCommunicationHistoryQueryHandler(IDbContext context, IMapper mapper, IOptions<SupportSettings> settings) : IRequestHandler<GetUserCommunicationHistoryQuery, CommunicationHistoryDto>
{
    private readonly SupportSettings supportConfig = settings.Value;

    public async Task<CommunicationHistoryDto> Handle(GetUserCommunicationHistoryQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        IQueryable<CustomerCommunication> query = context.Set<CustomerCommunication>()
            .AsNoTracking()
            .Where(c => c.UserId == request.UserId);

        var totalCommunications = await query.CountAsync(cancellationToken);
        var communicationsByType = await query
            .GroupBy(c => c.CommunicationType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
        var communicationsByChannel = await query
            .GroupBy(c => c.Channel)
            .Select(g => new { Channel = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Channel, x => x.Count, cancellationToken);
        var lastCommunicationDate = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => (DateTime?)c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        // Get recent communications
        var recent = await query
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .OrderByDescending(c => c.CreatedAt)
            .Take(supportConfig.DashboardRecentTicketsCount)
            .ToListAsync(cancellationToken);

        var history = new CommunicationHistoryDto(
            request.UserId,
            $"{user.FirstName} {user.LastName}",
            totalCommunications,
            communicationsByType,
            communicationsByChannel,
            mapper.Map<List<CustomerCommunicationDto>>(recent),
            lastCommunicationDate
        );

        return history;
    }
}
