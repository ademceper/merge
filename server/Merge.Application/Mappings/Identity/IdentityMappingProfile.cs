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
    }
}
