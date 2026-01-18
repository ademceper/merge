using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Payment;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetGiftCardByCode;

public class GetGiftCardByCodeQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetGiftCardByCodeQuery, GiftCardDto?>
{
    public async Task<GiftCardDto?> Handle(GetGiftCardByCodeQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        return giftCard is null ? null : mapper.Map<GiftCardDto>(giftCard);
    }
}
