using AutoMapper;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Underwriting;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Underwritings;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UnderwritingMappingsProfile : Profile
    {
        public UnderwritingMappingsProfile()
        {
            CreateMap<MatchedUnderwritingRequestDto, MatchedUnderwritingRequest>();

            CreateMap<UpdateUnderwritingRequirementRequest, UpdateUnderwritingRequirementCommand>();

            CreateMap<CreateUnderwritingRequestRequest, CreateUnderwritingRequestCommand>();

            CreateMap<CreateUnderwritingRequirementRequest, CreateUnderwritingRequirementCommand>();

            CreateMap<UpdateUnderwritingRequestRequest, UpdateUnderwritingRequestCommand>();

            CreateMap<UnderwritingRequirementWithRequestDto, UnderwritingRequirementWithRequest>();

            CreateMap<UnderwritingRequestDto, UnderwritingRequest>();

            CreateMap<UnderwritingAttachmentSummaryDto, UnderwritingAttachmentSummary>();

            CreateMap<MatchedUnderwritingRequirementDto, MatchedUnderwritingRequirement>();

            CreateMap<Page<MatchedUnderwritingRequestDto>, PagedSearchResult<MatchedUnderwritingRequest>>();

            CreateMap<SearchUnderwritingRequest, SearchUnderwritingRequestsQuery>();

            CreateMap<SearchUnderwritingRequest, SearchPendingUnderwritingRequestsQuery>();

            CreateMap<SearchUnderwritingRequest, SearchPendingUnderwritingRequirementsQuery>();

            CreateMap<Page<MatchedUnderwritingRequirementDto>, PagedSearchResult<MatchedUnderwritingRequirement>>();
        }
    }
}