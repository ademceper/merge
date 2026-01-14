using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.CreateB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
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

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            logger.LogWarning("User not found with Id: {UserId}", request.UserId);
            throw new Merge.Application.Exceptions.NotFoundException("Kullanıcı", request.UserId);
        }

        var organization = await context.Set<Merge.Domain.Modules.Identity.Organization>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            logger.LogWarning("Organization not found with Id: {OrganizationId}", request.OrganizationId);
            throw new Merge.Application.Exceptions.NotFoundException("Organizasyon", Guid.Empty);
        }

        // Check if user is already a B2B user for this organization
        var existing = await context.Set<B2BUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.OrganizationId == request.OrganizationId, cancellationToken);

        if (existing != null)
        {
            logger.LogWarning("User {UserId} is already a B2B user for organization {OrganizationId}",
                request.UserId, request.OrganizationId);
            throw new Merge.Application.Exceptions.BusinessException("Kullanıcı zaten bu organizasyon için B2B kullanıcısı.");
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Factory Method kullanımı
        var b2bUser = B2BUser.Create(
            request.UserId,
            request.OrganizationId,
            organization,
            request.EmployeeId,
            request.Department,
            request.JobTitle,
            request.CreditLimit);

        if (request.Settings != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            b2bUser.UpdateSettings(JsonSerializer.Serialize(request.Settings, JsonOptions));
        }

        await context.Set<B2BUser>().AddAsync(b2bUser, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully created B2B user with Id: {B2BUserId}", b2bUser.Id);

        // ✅ PERFORMANCE: Reload with Include for AutoMapper
        // ✅ PERFORMANCE: AsSplitQuery to avoid Cartesian Explosion (multiple Include'lar)
        b2bUser = await context.Set<B2BUser>()
            .AsNoTracking()
            .AsSplitQuery() // ✅ BOLUM 8.1.4: Query Splitting - Multiple Include'lar için
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == b2bUser.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return mapper.Map<B2BUserDto>(b2bUser!);
    }
}

