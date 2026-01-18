using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Application.Exceptions;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreateB2BUser;

public class CreateB2BUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<CreateB2BUserCommandHandler> logger) : IRequestHandler<CreateB2BUserCommand, B2BUserDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<B2BUserDto> Handle(CreateB2BUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Creating B2B user for UserId: {UserId}, OrganizationId: {OrganizationId}",
            request.UserId, request.OrganizationId);

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found with Id: {UserId}", request.UserId);
            throw new NotFoundException("Kullanıcı", request.UserId);
        }

        var organization = await context.Set<OrganizationEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization is null)
        {
            logger.LogWarning("Organization not found with Id: {OrganizationId}", request.OrganizationId);
            throw new NotFoundException("Organizasyon", Guid.Empty);
        }

        // Check if user is already a B2B user for this organization
        var existing = await context.Set<B2BUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.OrganizationId == request.OrganizationId, cancellationToken);

        if (existing is not null)
        {
            logger.LogWarning("User {UserId} is already a B2B user for organization {OrganizationId}",
                request.UserId, request.OrganizationId);
            throw new BusinessException("Kullanıcı zaten bu organizasyon için B2B kullanıcısı.");
        }

        var b2bUser = B2BUser.Create(
            request.UserId,
            request.OrganizationId,
            organization,
            request.EmployeeId,
            request.Department,
            request.JobTitle,
            request.CreditLimit);

        if (request.Settings is not null)
        {
            b2bUser.UpdateSettings(JsonSerializer.Serialize(request.Settings, JsonOptions));
        }

        await context.Set<B2BUser>().AddAsync(b2bUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created B2B user with Id: {B2BUserId}", b2bUser.Id);

        b2bUser = await context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == b2bUser.Id, cancellationToken);

        return mapper.Map<B2BUserDto>(b2bUser!);
    }
}

