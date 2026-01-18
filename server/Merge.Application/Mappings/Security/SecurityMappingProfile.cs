using AutoMapper;
using Merge.Application.DTOs.Security;
using Merge.Domain.SharedKernel;
using Merge.Domain.Modules.Payment;
using Merge.Domain.Modules.Ordering;
using Merge.Domain.Modules.Identity;
using System.Text.Json;

namespace Merge.Application.Mappings.Security;

public class SecurityMappingProfile : Profile
{
    public SecurityMappingProfile()
    {
        // Security mappings
        CreateMap<OrderVerification, OrderVerificationDto>()
        .ForMember(dest => dest.OrderNumber, opt => opt.MapFrom(src => 
        src.Order != null ? src.Order.OrderNumber : string.Empty))
        .ForMember(dest => dest.VerifiedByName, opt => opt.MapFrom(src => 
        src.VerifiedBy != null ? $"{src.VerifiedBy.FirstName} {src.VerifiedBy.LastName}" : null))
        .ForMember(dest => dest.VerificationType, opt => opt.MapFrom(src => src.VerificationType.ToString()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<PaymentFraudPrevention, PaymentFraudPreventionDto>()
        .ForMember(dest => dest.PaymentTransactionId, opt => opt.MapFrom(src =>
        src.Payment != null ? src.Payment.TransactionId : string.Empty))
        .ForMember(dest => dest.CheckType, opt => opt.MapFrom(src => src.CheckType.ToString()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        .AfterMap((src, dest) =>
        {
        dest.CheckResult = !string.IsNullOrEmpty(src.CheckResult)
        ? JsonSerializer.Deserialize<FraudDetectionMetadataDto>(src.CheckResult)
        : null;
        });

        CreateMap<AccountSecurityEvent, AccountSecurityEventDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : string.Empty))
        .ForMember(dest => dest.ActionTakenByName, opt => opt.MapFrom(src =>
        src.ActionTakenBy != null ? $"{src.ActionTakenBy.FirstName} {src.ActionTakenBy.LastName}" : null))
        .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.ToString()))
        .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
        .AfterMap((src, dest) =>
        {
        dest.Details = !string.IsNullOrEmpty(src.Details)
        ? JsonSerializer.Deserialize<SecurityEventMetadataDto>(src.Details)
        : null;
        });

        CreateMap<SecurityAlert, SecurityAlertDto>()
        .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
        src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : null))
        .ForMember(dest => dest.AcknowledgedByName, opt => opt.MapFrom(src =>
        src.AcknowledgedBy != null ? $"{src.AcknowledgedBy.FirstName} {src.AcknowledgedBy.LastName}" : null))
        .ForMember(dest => dest.ResolvedByName, opt => opt.MapFrom(src =>
        src.ResolvedBy != null ? $"{src.ResolvedBy.FirstName} {src.ResolvedBy.LastName}" : null))
        .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        .AfterMap((src, dest) =>
        {
        dest.Metadata = !string.IsNullOrEmpty(src.Metadata)
        ? JsonSerializer.Deserialize<SecurityEventMetadataDto>(src.Metadata)
        : null;
        });


    }
}
