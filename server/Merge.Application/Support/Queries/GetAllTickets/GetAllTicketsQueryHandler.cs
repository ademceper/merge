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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetAllTicketsQueryHandler : IRequestHandler<GetAllTicketsQuery, PagedResult<SupportTicketDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;
    private readonly SupportSettings _settings;

    public GetAllTicketsQueryHandler(
        IDbContext context,
        IMapper mapper,
        IOptions<SupportSettings> settings)
    {
        _context = context;
        _mapper = mapper;
        _settings = settings.Value;
    }

    public async Task<PagedResult<SupportTicketDto>> Handle(GetAllTicketsQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 3.4: Pagination limit kontrolü (ZORUNLU)
        // ✅ BOLUM 12.0: Magic Number'ları Configuration'a Taşıma
        var pageSize = request.PageSize > 0 && request.PageSize <= _settings.MaxPageSize 
            ? request.PageSize 
            : _settings.DefaultPageSize;
        var page = request.Page > 0 ? request.Page : 1;

        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
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

        // ✅ PERFORMANCE: ticketIds'i database'de oluştur, memory'de işlem YASAK
        var ticketIds = await query
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => t.Id)
            .ToListAsync(cancellationToken);

        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        var tickets = await _context.Set<SupportTicket>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(t => t.User)
            .Include(t => t.Order)
            .Include(t => t.Product)
            .Include(t => t.AssignedTo)
            .Where(t => ticketIds.Contains(t.Id))
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.CreatedAt)
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

        return new PagedResult<SupportTicketDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
