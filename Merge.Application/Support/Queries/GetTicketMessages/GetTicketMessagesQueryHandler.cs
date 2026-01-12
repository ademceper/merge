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

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetTicketMessagesQueryHandler : IRequestHandler<GetTicketMessagesQuery, IEnumerable<TicketMessageDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetTicketMessagesQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TicketMessageDto>> Handle(GetTicketMessagesQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsNoTracking + Removed manual !m.IsDeleted (Global Query Filter)
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        IQueryable<TicketMessage> query = _context.Set<TicketMessage>()
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

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<TicketMessageDto>>(messages);
    }
}
