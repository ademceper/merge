using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Microsoft.Extensions.Options;
using Merge.Application.DTOs.Support;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetAllTickets;

public class GetAllTicketsQueryHandler(IDbContext context, IMapper mapper, IOptions<SupportSettings> settings) : IRequestHandler<GetAllTicketsQuery, PagedResult<SupportTicketDto>>
{
    public async Task<PagedResult<SupportTicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        var pageSize = request.PageSize > 0 && request.PageSize <= settings.Value.MaxPageSize
            ? request.PageSize
            : settings.Value.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        IQueryable<SupportTicket> query = context.Set<SupportTicket>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo);

        if (!string.IsNullOrEmpty(request.Status))
        {
            var ticketStatus = Enum.Parse<TicketStatus>(request.Status, true);
            query = query.Where(t => t.Status == ticketStatus);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            var ticketCategory = Enum.Parse<TicketCategory>(request.Category, true);
            query = query.Where(t => t.Category == ticketCategory);
        }

        if (request.AssignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedToId == request.AssignedToId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var paginatedTicketsQuery = query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);

        var tickets = await paginatedTicketsQuery
            .ToListAsync(cancellationToken);

        var ticketIdsSubquery = from t in paginatedTicketsQuery select t.Id;
        var messagesDict = await context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => ticketIdsSubquery.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIdsSubquery.Contains(a.TicketId))
            .GroupBy(a => a.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Attachments = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Attachments, cancellationToken);

        var dtos = new List<SupportTicketDto>(tickets.Count);
        foreach (var ticket in tickets)
        {
            var dto = mapper.Map<SupportTicketDto>(ticket);
            
            IReadOnlyList<TicketMessageDto> messages;
            if (messagesDict.TryGetValue(ticket.Id, out var messageList))
            {
                messages = mapper.Map<List<TicketMessageDto>>(messageList).AsReadOnly();
            }
            else
            {
                messages = Array.Empty<TicketMessageDto>().AsReadOnly();
            }
            
            IReadOnlyList<TicketAttachmentDto> attachments;
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachmentList))
            {
                attachments = mapper.Map<List<TicketAttachmentDto>>(attachmentList).AsReadOnly();
            }
            else
            {
                attachments = Array.Empty<TicketAttachmentDto>().AsReadOnly();
            }
            
            dtos.Add(dto with { Messages = messages, Attachments = attachments });
        }

        return new PagedResult<SupportTicketDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
