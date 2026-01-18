using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Marketing;
using Merge.Application.Interfaces;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Marketing;
using EmailSubscriber = Merge.Domain.Modules.Marketing.EmailSubscriber;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Marketing.Queries.GetEmailSubscriberById;

public class GetEmailSubscriberByIdQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetEmailSubscriberByIdQuery, EmailSubscriberDto?>
{
    public async Task<EmailSubscriberDto?> Handle(GetEmailSubscriberByIdQuery request, CancellationToken cancellationToken)
    {
        var subscriber = await context.Set<EmailSubscriber>()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        return subscriber is not null ? mapper.Map<EmailSubscriberDto>(subscriber) : null;
    }
}
