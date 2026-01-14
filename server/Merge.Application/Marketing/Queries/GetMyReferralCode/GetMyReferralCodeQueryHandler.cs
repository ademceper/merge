using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using IDbContext = Merge.Application.Interfaces.IDbContext;

namespace Merge.Application.Marketing.Queries.GetMyReferralCode;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 2.0: CQRS - Query handler'lar SADECE okuma yapmalı, entity oluşturma YASAK
// ✅ BOLUM 7.1.8: Primary Constructors (C# 12) - Modern .NET 9 feature
public class GetMyReferralCodeQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetMyReferralCodeQuery, ReferralCodeDto?>
{
    public async Task<ReferralCodeDto?> Handle(GetMyReferralCodeQuery request, CancellationToken cancellationToken)
    {
        // ✅ BOLUM 2.0: CQRS - Query handler'lar SADECE okuma yapmalı, entity oluşturma YASAK
        // Entity oluşturma mantığı controller seviyesinde veya ayrı bir command handler'da olmalı
        var code = await context.Set<ReferralCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (code == null)
        {
            // ✅ BOLUM 2.0: CQRS - Query handler null döndürür, controller seviyesinde CreateReferralCodeCommand çağrılabilir
            return null;
        }

        return mapper.Map<ReferralCodeDto>(code);
    }
}
