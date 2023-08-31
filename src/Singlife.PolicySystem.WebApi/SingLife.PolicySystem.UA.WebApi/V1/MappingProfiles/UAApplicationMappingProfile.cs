using AutoMapper;
using SingLife.PolicySystem.UA.UseCases.Applications;
using SingLife.PolicySystem.UA.UseCases.Checklists;
using SingLife.ULTracker.WebAPI.Contracts.UA.Applications;
using SingLife.ULTracker.WebAPI.Contracts.UA.Checklist;
using UAApplicationSummaryContract = SingLife.ULTracker.WebAPI.Contracts.UA.Applications.UAApplicationSummary;

namespace SingLife.PolicySystem.UA.WebApi.V1.MappingProfiles
{
    public class UAApplicationMappingProfile : Profile
    {
        public UAApplicationMappingProfile()
        {
            CreateMap<UAApplicationSummaryDto, UAApplicationSummaryContract>()
                .ForMember(dest => dest.GeneralInformation, opt => opt.MapFrom(src => src.GeneralInfo));

            CreateMap<GeneralInformationDto, GeneralInformation>();

            CreateMap<ChecklistItemForUpdateDto, ChecklistItemForUpdate>();
        }
    }
}
