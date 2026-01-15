using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Content;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetLiveChatSessionMessages;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
public class GetLiveChatSessionMessagesQueryHandler : IRequestHandler<GetLiveChatSessionMessagesQuery, IEnumerable<LiveChatMessageDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetLiveChatSessionMessagesQueryHandler(
        IDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LiveChatMessageDto>> Handle(GetLiveChatSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        // ✅ PERFORMANCE: AsSplitQuery - Multiple Include'lar için query splitting (Cartesian Explosion önleme)
        // Not: Şu anda sadece 1 Include var ama gelecekte ek Include'lar eklenebilir
        var messages = await _context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.SessionId == request.SessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullan
        return _mapper.Map<IEnumerable<LiveChatMessageDto>>(messages);
    }
}
