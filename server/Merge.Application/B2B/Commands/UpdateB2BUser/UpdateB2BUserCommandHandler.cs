using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Merge.Application.Interfaces;
using Merge.Domain.Entities;
using Merge.Domain.Enums;
using static Merge.Application.DTOs.B2B.B2BUserSettingsDto;
using Merge.Domain.Interfaces;
using Merge.Domain.Modules.Identity;
using IDbContext = Merge.Application.Interfaces.IDbContext;
using IUnitOfWork = Merge.Application.Interfaces.IUnitOfWork;

namespace Merge.Application.B2B.Commands.UpdateB2BUser;

public class UpdateB2BUserCommandHandler(
    IDbContext context,
    IUnitOfWork unitOfWork,
    ILogger<UpdateB2BUserCommandHandler> logger) : IRequestHandler<UpdateB2BUserCommand, bool>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async Task<bool> Handle(UpdateB2BUserCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Updating B2B user. B2BUserId: {B2BUserId}", request.Id);

        var b2bUser = await context.Set<B2BUser>()
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken);

        if (b2bUser == null)
        {
            logger.LogWarning("B2B user not found with Id: {B2BUserId}", request.Id);
            return false;
        }

        b2bUser.UpdateProfile(request.Dto.EmployeeId, request.Dto.Department, request.Dto.JobTitle);
        
        if (!string.IsNullOrEmpty(request.Dto.Status))
        {
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
            b2bUser.UpdateSettings(JsonSerializer.Serialize(request.Dto.Settings, JsonOptions));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("B2B user updated successfully. B2BUserId: {B2BUserId}", request.Id);
        return true;
    }
}

