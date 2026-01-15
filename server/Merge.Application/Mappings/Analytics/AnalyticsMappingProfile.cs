using AutoMapper;
using Merge.Application.DTOs.Analytics;
using Merge.Domain.Modules.Analytics;

namespace Merge.Application.Mappings.Analytics;

public class AnalyticsMappingProfile : Profile
{
    public AnalyticsMappingProfile()
    {
        // Analytics mappings
        CreateMap<Report, ReportDto>()
        .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
        .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format.ToString()))
        .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
        .ForMember(dest => dest.GeneratedBy, opt => opt.MapFrom(src => 
        src.GeneratedByUser != null ? $"{src.GeneratedByUser.FirstName} {src.GeneratedByUser.LastName}" : string.Empty))
        .ForMember(dest => dest.GeneratedByUserId, opt => opt.MapFrom(src => src.GeneratedBy));

        CreateMap<ReportSchedule, ReportScheduleDto>()
        .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
        .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.Frequency.ToString()))
        .ForMember(dest => dest.Format, opt => opt.MapFrom(src => src.Format.ToString()));

        CreateMap<DashboardMetric, DashboardMetricDto>()
        .ForMember(dest => dest.ValueFormatted, opt => opt.MapFrom(src => 
        src.ValueFormatted ?? src.Value.ToString("N2")));


    }
}
