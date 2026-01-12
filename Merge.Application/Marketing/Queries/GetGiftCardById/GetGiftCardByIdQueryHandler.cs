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

public class GetGiftCardByIdQueryHandler : IRequestHandler<GetGiftCardByIdQuery, GiftCardDto?>
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public GetGiftCardByIdQueryHandler(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<GiftCardDto?> Handle(GetGiftCardByIdQuery request, CancellationToken cancellationToken)
    {
        var giftCard = await _context.Set<GiftCard>()
            .AsNoTracking()
            .FirstOrDefaultAsync(gc => gc.Id == request.Id, cancellationToken);

        return giftCard == null ? null : _mapper.Map<GiftCardDto>(giftCard);
    }
}
