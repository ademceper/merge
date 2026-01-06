using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using static Merge.Application.DTOs.B2B.B2BUserSettingsDto;

namespace Merge.Application.B2B.Commands.UpdateB2BUser;

// ✅ BOLUM 2.0: MediatR + CQRS pattern (ZORUNLU)
// ✅ BOLUM 1.1: Clean Architecture - Handler direkt IDbContext kullanıyor (Service layer bypass)
public class UpdateB2BUserCommandHandler : IRequestHandler<UpdateB2BUserCommand, bool>
{
    private readonly IDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateB2BUserCommandHandler> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public UpdateB2BUserCommandHandler(
        IDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<UpdateB2BUserCommandHandler> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateB2BUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating B2B user. B2BUserId: {B2BUserId}", request.Id);

        // ✅ FIX: Use FirstOrDefaultAsync without manual IsDeleted check (Global Query Filter handles it)
        var b2bUser = await _context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (b2bUser == null)
        {
            _logger.LogWarning("B2B user not found with Id: {B2BUserId}", request.Id);
            return false;
        }

        // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
        b2bUser.UpdateProfile(request.Dto.EmployeeId, request.Dto.Department, request.Dto.JobTitle);
        
        if (!string.IsNullOrEmpty(request.Dto.Status))
        {
            // ✅ BOLUM 1.2: Enum kullanımı (string Status YASAK)
            if (Enum.TryParse<EntityStatus>(request.Dto.Status, true, out var statusEnum))
            {
                b2bUser.UpdateStatus(statusEnum);
            }
        }
        
        if (request.Dto.CreditLimit.HasValue)
        {
            b2bUser.UpdateCreditLimit(request.Dto.CreditLimit.Value);
        }
        
        if (request.Dto.Settings != null)
        {
            // ✅ BOLUM 1.1: Rich Domain Model - Entity method kullanımı
            b2bUser.UpdateSettings(JsonSerializer.Serialize(request.Dto.Settings, JsonOptions));
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("B2B user updated successfully. B2BUserId: {B2BUserId}", request.Id);
        return true;
    }
}

