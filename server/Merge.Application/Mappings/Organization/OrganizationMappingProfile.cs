using AutoMapper;
using Merge.Application.DTOs.Organization;
using Merge.Domain.Modules.Identity;
using OrganizationEntity = Merge.Domain.Modules.Identity.Organization;
using System.Text.Json;

namespace Merge.Application.Mappings.Organization;

public class OrganizationMappingProfile : Profile
{
    public OrganizationMappingProfile()
    {
        // Organization mappings
        // ✅ BOLUM 7.1.5: Records - ConvertUsing ile record mapping (immutable DTOs + error handling)
        // ✅ FIX: Expression tree limitation - ConvertUsing kullanıyoruz (statement body destekleniyor)
        CreateMap<OrganizationEntity, OrganizationDto>()
        .ConvertUsing((src, context) =>
        {
        OrganizationSettingsDto? settings = null;
        if (!string.IsNullOrEmpty(src.Settings))
        {
        try
        {
        settings = JsonSerializer.Deserialize<OrganizationSettingsDto>(src.Settings);
        }
        catch
        {
        // ✅ ERROR HANDLING: JSON deserialize hatası - null bırak
        }
        }

        return new OrganizationDto(
        src.Id,
        src.Name,
        src.LegalName,
        src.TaxNumber,
        src.RegistrationNumber,
        src.Email,
        src.Phone,
        src.Website,
        src.Address,
        src.AddressLine2, // ✅ AddressLine2 property eklendi
        src.City,
        src.State,
        src.PostalCode,
        src.Country,
        src.Status.ToString(),
        src.IsVerified,
        src.VerifiedAt,
        settings, // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        0, // UserCount - Service layer'da set edilecek
        0, // TeamCount - Service layer'da set edilecek
        src.CreatedAt);
        });

        // ✅ FIX: Expression tree limitation - ConvertUsing kullanıyoruz (statement body destekleniyor)
        CreateMap<Team, TeamDto>()
        .ConvertUsing((src, context) =>
        {
        TeamSettingsDto? settings = null;
        if (!string.IsNullOrEmpty(src.Settings))
        {
        try
        {
        settings = JsonSerializer.Deserialize<TeamSettingsDto>(src.Settings);
        }
        catch
        {
        // ✅ ERROR HANDLING: JSON deserialize hatası - null bırak
        }
        }

        return new TeamDto(
        src.Id,
        src.OrganizationId,
        src.Organization != null ? src.Organization.Name : string.Empty,
        src.Name,
        src.Description,
        src.TeamLeadId,
        src.TeamLead != null ? $"{src.TeamLead.FirstName} {src.TeamLead.LastName}" : null,
        src.IsActive,
        settings, // ✅ SECURITY: Dictionary<string,object> yerine typed DTO kullaniyoruz
        0, // MemberCount - Service layer'da set edilecek
        src.CreatedAt);
        });

        CreateMap<TeamMember, TeamMemberDto>()
        .ConstructUsing(src => new TeamMemberDto(
        src.Id,
        src.TeamId,
        src.Team != null ? src.Team.Name : string.Empty,
        src.UserId,
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty,
        src.User != null ? src.User.Email ?? string.Empty : string.Empty,
        src.Role.ToString(),
        src.JoinedAt,
        src.IsActive));


    }
}
