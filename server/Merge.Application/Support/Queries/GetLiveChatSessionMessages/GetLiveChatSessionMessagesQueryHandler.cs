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

public class GetLiveChatSessionMessagesQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetLiveChatSessionMessagesQuery, IEnumerable<LiveChatMessageDto>>
{

    public async Task<IEnumerable<LiveChatMessageDto>> Handle(GetLiveChatSessionMessagesQuery request, CancellationToken cancellationToken)
    {
        // Not: Şu anda sadece 1 Include var ama gelecekte ek Include'lar eklenebilir
        var messages = await context.Set<LiveChatMessage>()
            .AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.SessionId == request.SessionId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<LiveChatMessageDto>>(messages);
    }
}
