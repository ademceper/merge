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

namespace Merge.Application.B2B.Queries.GetB2BUserById;

public class GetB2BUserByIdQueryHandler(
    IDbContext context,
    IMapper mapper,
    ILogger<GetB2BUserByIdQueryHandler> logger) : IRequestHandler<GetB2BUserByIdQuery, B2BUserDto?>
{

    public async Task<B2BUserDto?> Handle(GetB2BUserByIdQuery request, CancellationToken cancellationToken)
    {
        var b2bUser = await context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        return b2bUser != null ? mapper.Map<B2BUserDto>(b2bUser) : null;
    }
}

