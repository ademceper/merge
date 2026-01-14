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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTicketByNumberQueryHandler : IRequestHandler<GetTicketByNumberQuery, SupportTicketDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetTicketByNumberQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SupportTicketDto?> Handle(GetTicketByNumberQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !t.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<SupportTicket> query = _context.Set<SupportTicket>()
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

        var dto = _mapper.Map<SupportTicketDto>(ticket);
        
        // ✅ BOLUM 7.1.5: Records - IReadOnlyList kullanımı (immutability)
        var messages = _mapper.Map<List<TicketMessageDto>>(ticket.Messages).AsReadOnly();
        var attachments = _mapper.Map<List<TicketAttachmentDto>>(ticket.Attachments).AsReadOnly();
        
        // ✅ BOLUM 7.1.5: Records - Record'lar immutable, with expression kullan
        return dto with { Messages = messages, Attachments = attachments };
    }
}
