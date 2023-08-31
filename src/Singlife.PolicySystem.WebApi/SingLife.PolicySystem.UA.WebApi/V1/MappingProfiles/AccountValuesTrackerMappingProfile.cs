using AutoMapper;
using SingLife.PolicySystem.UA.UseCases.AccountValues;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Ua.AccountValues;

namespace SingLife.PolicySystem.UA.WebApi.V1.MappingProfiles
{
    public class AccountValuesTrackerMappingProfile : Profile
    {
        public AccountValuesTrackerMappingProfile()
        {
            CreateMap<GetAccountValuesTrackerRecordsRequest, GetPagedAccountValuesTrackerRecordsQuery>()
                .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.PageIndex - 1))
                .ForMember(dest => dest.SearchTerm, opt => opt.Ignore())
                .ForMember(dest => dest.OrderBy, opt => opt.Ignore());

            CreateMap<AccountValuesTrackerRecordDto, AccountValuesTrackerRecord>();

            CreateMap<Page<AccountValuesTrackerRecordDto>, PagedSearchResult<AccountValuesTrackerRecord>>();

            CreateMap<RecalculateAccountValuesForTrackedPoliciesRequest, RecalculateLeftoverAccountValuesOfTerminatedPoliciesCommand>();
        }
    }
}