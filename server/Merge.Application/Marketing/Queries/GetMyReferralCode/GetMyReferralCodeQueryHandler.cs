using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

public class GetMyReferralCodeQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetMyReferralCodeQuery, ReferralCodeDto?>
{
    public async Task<ReferralCodeDto?> Handle(GetMyReferralCodeQuery request, CancellationToken cancellationToken)
    {
        // Entity oluşturma mantığı controller seviyesinde veya ayrı bir command handler'da olmalı
        var code = await context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (code is null)
        {
            return null;
        }

        return mapper.Map<ReferralCodeDto>(code);
    }
}
