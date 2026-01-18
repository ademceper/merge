using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using Merge.Application.DTOs.Support;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using Merge.Domain.Modules.Support;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Support.Queries.GetCustomerCommunication;

public class GetCustomerCommunicationQueryHandler(IDbContext context, IMapper mapper) : IRequestHandler<GetCustomerCommunicationQuery, CustomerCommunicationDto?>
{

    public async Task<CustomerCommunicationDto?> Handle(GetCustomerCommunicationQuery request, CancellationToken cancellationToken)
    {
        var query = context.Set<CustomerCommunication>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.SentBy)
            .Where(c => c.Id == request.CommunicationId);

        if (request.UserId.HasValue)
        {
            query = query.Where(c => c.UserId == request.UserId.Value);
        }

        var communication = await query.FirstOrDefaultAsync(cancellationToken);

        return communication is not null ? mapper.Map<CustomerCommunicationDto>(communication) : null;
    }
}
