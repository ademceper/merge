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

namespace Merge.Application.Marketing.Queries.GetGiftCardById;

public class GetGiftCardByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetGiftCardByIdQuery, GiftCardDto?>
{
    public async Task<GiftCardDto?> Handle(GetGiftCardByIdQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == request.Id, cancellationToken);

        return giftCard is null ? null : mapper.Map<GiftCardDto>(giftCard);
    }
}
