using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;

namespace Merge.Application.Marketing.Queries.GetGiftCardByCode;

public class GetGiftCardByCodeQueryHandler : IRequestHandler<GetGiftCardByCodeQuery, GiftCardDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetGiftCardByCodeQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GiftCardDto?> Handle(GetGiftCardByCodeQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Code == request.Code, cancellationToken);

        return giftCard == null ? null : _mapper.Map<GiftCardDto>(giftCard);
    }
}
