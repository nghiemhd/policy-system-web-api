using AutoMapper;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.LongRunningTasks;
using SingLife.ULTracker.UseCases.LongRunningTasks.ReportingTasks;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.LongRunningTasks;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class LongRunningTaskMappingsProfile : Profile
    {
        public LongRunningTaskMappingsProfile()
        {
            CreateMap<GenerateMassPolicyStatementsRequest, GenerateMassPolicyStatementsCommand>();

            CreateMap<SearchTasksRequest, SearchMassPolicyStatementTasksQuery>()
                .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.CurrentPage - 1))
                .ForMember(dest => dest.SearchTerm, opt => opt.Ignore())
                .ForMember(dest => dest.OrderBy, opt => opt.Ignore());

            CreateMap<Page<ReportingTaskDto>, PagedSearchResult<ReportingTask>>();

            CreateMap<Page<MassPolicyStatementTaskDto>, PagedSearchResult<MassPolicyStatementTask>>();
        }
    }
}