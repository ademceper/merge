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

// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetGiftCardByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetGiftCardByIdQuery, GiftCardDto?>
{
    public async Task<GiftCardDto?> Handle(GetGiftCardByIdQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == request.Id, cancellationToken);

        return giftCard == null ? null : mapper.Map<GiftCardDto>(giftCard);
    }
}
