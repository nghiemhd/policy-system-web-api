using AutoMapper;
using SingLife.ULTracker.UseCases.Ulpb.V1.Applications;
using SingLife.ULTracker.UseCases.Ulpb.V1.Applications.DTOs;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.ApplicationChecklists;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UlpbApplicationChecklistMappingProfile : Profile
    {
        public UlpbApplicationChecklistMappingProfile()
        {
            CreateMap<UlpbApplicationChecklistDto, UlpbApplicationChecklist>();

            CreateMap<UlpbApplicationChecklistItemDto, UlpbApplicationChecklistItem>();

            CreateMap<ChangeChecklistItemStatusRequest, ManuallyChangeChecklistItemStatusCommand>();
        }
    }
}