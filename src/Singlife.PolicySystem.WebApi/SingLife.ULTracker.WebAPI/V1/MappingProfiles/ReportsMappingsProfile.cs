using AutoMapper;
using SingLife.ULTracker.UseCases.Reports.Accounting;
using SingLife.ULTracker.UseCases.Reports.ME;
using SingLife.ULTracker.WebAPI.Contracts.Reports.Accounting;
using SingLife.ULTracker.WebAPI.Contracts.Reports.ME;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class ReportsMappingsProfile : Profile
    {
        public ReportsMappingsProfile()
        {
            CreateMap<ExportAccountingReportToExcelRequest, GetAccountingReportInputQuery>()
                .ForMember(dest => dest.FromMonth, opt => opt.MapFrom(src => src.FromMonth.Month))
                .ForMember(dest => dest.FromYear, opt => opt.MapFrom(src => src.FromMonth.Year))
                .ForMember(dest => dest.ToMonth, opt => opt.MapFrom(src => src.ToMonth.Month))
                .ForMember(dest => dest.ToYear, opt => opt.MapFrom(src => src.ToMonth.Year));

            CreateMap<ExportMEReportToExcelRequest, GetMEReportInputQuery>();
        }
    }
}