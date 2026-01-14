using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Catalog;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetUnassignedTickets;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetUnassignedTicketsQueryHandler : IRequestHandler<GetUnassignedTicketsQuery, IEnumerable<SupportTicketDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetUnassignedTicketsQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupportTicketDto>> Handle(GetUnassignedTicketsQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        var ticketIds = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Where(t => t.AssignedToId == null && t.Status != TicketStatus.Closed)
            .OrderByDescending(t => t.Priority)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: Batch load messages and attachments for all tickets
        var messagesDict = await _context.Set<TicketMessage>()
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => ticketIds.Contains(m.TicketId))
            .GroupBy(m => m.TicketId)
            .Select(g => new
            {
                TicketId = g.Key,
                Messages = g.ToList()
            })
            .ToDictionaryAsync(x => x.TicketId, x => x.Messages, cancellationToken);

        var attachmentsDict = await _context.Set<TicketAttachment>()
            .AsNoTracking()
            .Where(a => ticketIds.Contains(a.TicketId))
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
            var dto = _mapper.Map<SupportTicketDto>(ticket);
            
            // ✅ BOLUM 7.1.5: Records - IReadOnlyList kullanımı (immutability)
            IReadOnlyList<TicketMessageDto> messages;
            if (messagesDict.TryGetValue(ticket.Id, out var messageList))
            {
                messages = _mapper.Map<List<TicketMessageDto>>(messageList).AsReadOnly();
            }
            else
            {
                messages = Array.Empty<TicketMessageDto>().AsReadOnly();
            }
            
            IReadOnlyList<TicketAttachmentDto> attachments;
            if (attachmentsDict.TryGetValue(ticket.Id, out var attachmentList))
            {
                attachments = _mapper.Map<List<TicketAttachmentDto>>(attachmentList).AsReadOnly();
            }
            else
            {
                attachments = Array.Empty<TicketAttachmentDto>().AsReadOnly();
            }
            
            // ✅ BOLUM 7.1.5: Records - Record'lar immutable, with expression kullan
            dtos.Add(dto with { Messages = messages, Attachments = attachments });
        }

        return dtos;
    }
}
