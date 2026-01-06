using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.DTOs.B2B;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using AutoMapper;

namespace Merge.Application.B2B.Commands.CreateB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class CreateB2BUserCommandHandler : IRequestHandler<CreateB2BUserCommand, B2BUserDto>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateB2BUserCommandHandler> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public CreateB2BUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateB2BUserCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<B2BUserDto> Handle(CreateB2BUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating B2B user for UserId: {UserId}, OrganizationId: {OrganizationId}",
            request.UserId, request.OrganizationId);

        // ✅ PERFORMANCE: AsNoTracking for read-only queries
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            _logger.LogWarning("User not found with Id: {UserId}", request.UserId);
            throw new Merge.Application.Exceptions.NotFoundException("Kullanıcı", request.UserId);
        }

        var organization = await _context.Set<Organization>()
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            _logger.LogWarning("Organization not found with Id: {OrganizationId}", request.OrganizationId);
            throw new Merge.Application.Exceptions.NotFoundException("Organizasyon", Guid.Empty);
        }

        // Check if user is already a B2B user for this organization
        var existing = await _context.Set<B2BUser>()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.OrganizationId == request.OrganizationId, cancellationToken);

        if (existing != null)
        {
            _logger.LogWarning("User {UserId} is already a B2B user for organization {OrganizationId}",
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

        await _context.Set<B2BUser>().AddAsync(b2bUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Successfully created B2B user with Id: {B2BUserId}", b2bUser.Id);

        // ✅ PERFORMANCE: Reload with Include for AutoMapper
        b2bUser = await _context.Set<B2BUser>()
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Organization)
            .FirstOrDefaultAsync(b => b.Id == b2bUser.Id, cancellationToken);

        // ✅ ARCHITECTURE: AutoMapper kullanımı (manuel mapping yerine)
        return _mapper.Map<B2BUserDto>(b2bUser!);
    }
}

