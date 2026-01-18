using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Application.Interfaces;
using Merge.Application.Common;
using Merge.Application.Configuration;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Ordering;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.Security.Queries.GetAllVerifications;

public class GetAllVerificationsQueryHandler(IDbContext context, IMapper mapper, ILogger<GetAllVerificationsQueryHandler> logger, IOptions<PaginationSettings> paginationSettings) : IRequestHandler<GetAllVerificationsQuery, PagedResult<OrderVerificationDto>>
{
    private readonly PaginationSettings paginationConfig = paginationSettings.Value;


    public async Task<PagedResult<OrderVerificationDto>> Handle(GetAllVerificationsQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order verification'lar sorgulanıyor. Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.Status ?? "All", request.Page, request.PageSize);

        var pageSize = request.PageSize > paginationConfig.MaxPageSize ? paginationConfig.MaxPageSize : request.PageSize;
        var page = request.Page < 1 ? 1 : request.Page;

        IQueryable<OrderVerification> query = context.Set<OrderVerification>()
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Order)
            .Include(v => v.VerifiedBy);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<VerificationStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(v => v.Status == statusEnum);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var verifications = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var verificationDtos = mapper.Map<IEnumerable<OrderVerificationDto>>(verifications).ToList();

        logger.LogInformation("Order verification'lar bulundu. TotalCount: {TotalCount}, Page: {Page}, PageSize: {PageSize}, ReturnedCount: {ReturnedCount}",
            totalCount, page, pageSize, verificationDtos.Count);

        return new PagedResult<OrderVerificationDto>
        {
            Items = verificationDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
