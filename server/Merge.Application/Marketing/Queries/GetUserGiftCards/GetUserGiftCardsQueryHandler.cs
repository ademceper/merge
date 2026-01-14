using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Common;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetUserGiftCards;

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetUserGiftCardsQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetUserGiftCardsQuery, PagedResult<GiftCardDto>>
{
    public async Task<PagedResult<GiftCardDto>> Handle(GetUserGiftCardsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<GiftCard>()
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
            Items = mapper.Map<List<GiftCardDto>>(giftCards),
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
