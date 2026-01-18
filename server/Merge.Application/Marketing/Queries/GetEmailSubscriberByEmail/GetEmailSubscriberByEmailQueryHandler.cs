using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using Merge.Domain.ValueObjects;
using EmailSubscriber = Merge.Domain.Modules.Marketing.EmailSubscriber;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberByEmail;

public class GetEmailSubscriberByEmailQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetEmailSubscriberByEmailQuery, EmailSubscriberDto?>
{
    public async Task<EmailSubscriberDto?> Handle(GetEmailSubscriberByEmailQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => EF.Functions.ILike(s.Email, request.Email), cancellationToken);

        return subscriber != null ? mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }
}
