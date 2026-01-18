using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetTicketMessages;

public class GetTicketMessagesQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetTicketMessagesQuery, IEnumerable<TicketMessageDto>>
{

    public async Task<IEnumerable<TicketMessageDto>> Handle(GetTicketMessagesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<TicketMessage> query = context.Set<TicketMessage>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(m => m.User)
            .Include(m => m.Attachments)
            .Where(m => m.TicketId == request.TicketId);

        if (!request.IncludeInternal)
        {
            query = query.Where(m => !m.IsInternal);
        }

        var messages = await query
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<TicketMessageDto>>(messages);
    }
}
