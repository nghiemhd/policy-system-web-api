using AutoMapper;
using SingLife.PolicySystem.UA.UseCases.Checklists;
using SingLife.ULTracker.WebAPI.Contracts.UA.Checklist;

namespace SingLife.PolicySystem.UA.WebApi.V1.MappingProfiles
{
    public class UAChecklistMappingProfile : Profile
    {
        public UAChecklistMappingProfile()
        {
            CreateMap<UpdateChecklistItemRequest, ManuallyChangeChecklistItemCommand>();

            CreateMap<UpdateChecklistItemCommentRequest, UpdateChecklistItemCommentCommand>();
        }
    }
}