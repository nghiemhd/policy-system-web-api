using AutoMapper;
using SingLife.ULTracker.UseCases.UL.V1.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Policies.Documents;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class PolicyDocumentMappingsProfile : Profile
    {
        public PolicyDocumentMappingsProfile()
        {
            CreateMap<PrintPolicyStatementDocumentRequest, GetPolicyStatementDocumentDataQuery>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.From))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.To));

            CreateMap<PrintCommissionStatementDocumentRequest, GetCommissionStatementDocumentDataQuery>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.From))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.To));

            CreateMap<PrintWelcomeLetterAndSchedulesDocumentsRequest, GetWelcomeLetterAndSchedulesDocumentDataQuery>();

            CreateMap<PrintTermsOfAcceptanceDocumentRequest, GetTermsOfAcceptanceDocumentDataQuery>();
        }
    }
}