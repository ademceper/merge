using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetUserGiftCards;

public class GetUserGiftCardsQueryHandler : IRequestHandler<GetUserGiftCardsQuery, PagedResult<GiftCardDto>>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetUserGiftCardsQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<GiftCardDto>> Handle(GetUserGiftCardsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<GiftCard>()
            .AsNoTracking()
            .Where(gc => gc.PurchasedByUserId == request.UserId || gc.AssignedToUserId == request.UserId)
            .OrderByDescending(gc => gc.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var giftCards = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GiftCardDto>
        {
            Items = _mapper.Map<List<GiftCardDto>>(giftCards),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
