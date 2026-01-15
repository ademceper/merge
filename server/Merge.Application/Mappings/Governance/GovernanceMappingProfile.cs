using AutoMapper;
using Merge.Application.DTOs.Governance;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Content;

namespace Merge.Application.Mappings.Governance;

public class GovernanceMappingProfile : Profile
{
    public GovernanceMappingProfile()
    {
        // Governance mappings
        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // Governance mappings
        CreateMap<Policy, PolicyDto>()
        .ConstructUsing(src => new PolicyDto(
        src.Id,
        src.PolicyType,
        src.Title,
        src.Content,
        src.Version,
        src.IsActive,
        src.RequiresAcceptance,
        src.EffectiveDate,
        src.ExpiryDate,
        src.CreatedByUserId,
        src.CreatedBy != null ? $"{src.CreatedBy.FirstName} {src.CreatedBy.LastName}" : null,
        src.ChangeLog,
        src.Language,
        0, // AcceptanceCount - Set manually (computed)
        src.CreatedAt,
        src.UpdatedAt));

        CreateMap<PolicyAcceptance, PolicyAcceptanceDto>()
        .ConstructUsing(src => new PolicyAcceptanceDto(
        src.Id,
        src.PolicyId,
        src.Policy != null ? src.Policy.Title : string.Empty,
        src.UserId,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
        src.AcceptedVersion,
        src.IpAddress,
        src.AcceptedAt,
        src.IsActive));

        // ✅ BOLUM 7.1.5: Records - ConstructUsing ile record mapping
        // AuditLog mapping
        CreateMap<AuditLog, AuditLogDto>()
        .ConstructUsing(src => new AuditLogDto(
        src.Id,
        src.UserId,
        src.UserEmail,
        src.Action,
        src.EntityType,
        src.EntityId,
        src.TableName,
        src.PrimaryKey,
        src.OldValues,
        src.NewValues,
        src.Changes,
        src.IpAddress,
        src.UserAgent,
        src.Severity.ToString(),
        src.Module,
        src.IsSuccessful,
        src.ErrorMessage,
        src.CreatedAt));


    }
}
