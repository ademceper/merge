using AutoMapper;
using Merge.Application.DTOs.Identity;
using Merge.Application.DTOs.User;
using Merge.Domain.Modules.Identity;
using UserEntity = Merge.Domain.Modules.Identity.User;

namespace Merge.Application.Mappings.Identity;

public class IdentityMappingProfile : Profile
{
    public IdentityMappingProfile()
    {
        CreateMap<UserEntity, UserDto>()
            .ConstructUsing(src => new UserDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.Email ?? string.Empty,
                src.PhoneNumber ?? string.Empty,
                string.Empty)); // Role handler'da set edilecek

        CreateMap<Address, AddressDto>();
        CreateMap<AddressDto, Address>()
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Orders, opt => opt.Ignore());

        // User Preference mappings
        CreateMap<UserPreference, UserPreferenceDto>();

        // User Activity Log mappings
        CreateMap<UserActivityLog, UserActivityLogDto>()
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src =>
                src.User != null ? src.User.Email : "Anonymous"));

        CreateMap<CreateAddressDto, Address>();
        CreateMap<UpdateAddressDto, Address>();

        // TwoFactorAuth mappings
        CreateMap<TwoFactorAuth, TwoFactorStatusDto>()
            .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore()) // Set manually (MaskPhoneNumber)
            .ForMember(dest => dest.Email, opt => opt.Ignore()) // Set manually (MaskEmail)
            .ForMember(dest => dest.BackupCodesRemaining, opt => opt.Ignore()); // Set manually (Array.Length)

        // Permission mappings
        CreateMap<Permission, PermissionDto>();

        // Role mappings
        CreateMap<Role, RoleDto>()
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => 
                src.RolePermissions.Select(rp => rp.Permission)));

        // StoreRole mappings
        CreateMap<StoreRole, StoreRoleDto>()
            .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store.StoreName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email ?? string.Empty))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name ?? string.Empty));

        // OrganizationRole mappings
        CreateMap<OrganizationRole, OrganizationRoleDto>()
            .ForMember(dest => dest.OrganizationName, opt => opt.MapFrom(src => src.Organization.Name))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email ?? string.Empty))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name ?? string.Empty));

        // StoreCustomerRole mappings
        CreateMap<StoreCustomerRole, StoreCustomerRoleDto>()
            .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store.StoreName))
            .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email ?? string.Empty))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name ?? string.Empty));
    }
}
