using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Queries.GetB2BUserByUserId;

public class GetB2BUserByUserIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetB2BUserByUserIdQueryHandler> logger) : IRequestHandler<GetB2BUserByUserIdQuery, B2BUserDto?>
{

    public async Task<B2BUserDto?> Handle(GetB2BUserByUserIdQuery request, CancellationToken cancellationToken)
    {
        var b2bUser = await context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.UserId == request.UserId, cancellationToken);

        return b2bUser is not null ? mapper.Map<B2BUserDto>(b2bUser) : null;
    }
}

