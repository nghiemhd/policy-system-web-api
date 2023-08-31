using AutoMapper;
using SingLife.ULTracker.UseCases.Compliances;
using SingLife.ULTracker.WebAPI.Contracts.Compliances;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class ComplianceMappingsProfile : Profile
    {
        public ComplianceMappingsProfile()
        {
            CreateMap<BankruptcyDto, Bankruptcy>()
                .ForMember(dest => dest.Action, opt => opt.Ignore());
        }
    }
}