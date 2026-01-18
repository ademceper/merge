using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetTicketByNumber;

public class GetTicketByNumberQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetTicketByNumberQuery, SupportTicketDto?>
{

    public async Task<SupportTicketDto?> Handle(GetTicketByNumberQuery request, CancellationToken cancellationToken)
    {
        IQueryable<SupportTicket> query = context.Set<SupportTicket>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Include(t => t.Messages)
                .ThenInclude(m => m.User)
            .Include(t => t.Attachments)
            .Where(t => t.TicketNumber == request.TicketNumber);

        if (request.UserId.HasValue)
        {
            query = query.Where(t => t.UserId == request.UserId.Value);
        }

        var ticket = await query.FirstOrDefaultAsync(cancellationToken);

        if (ticket == null) return null;

        var dto = mapper.Map<SupportTicketDto>(ticket);
        
        var messages = mapper.Map<List<TicketMessageDto>>(ticket.Messages).AsReadOnly();
        var attachments = mapper.Map<List<TicketAttachmentDto>>(ticket.Attachments).AsReadOnly();
        
        return dto with { Messages = messages, Attachments = attachments };
    }
}
