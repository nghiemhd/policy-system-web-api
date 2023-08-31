using AutoMapper;
using SingLife.ULTracker.UseCases.Underwriting;
using SingLife.ULTracker.WebAPI.Contracts.UnderwritingAttachments;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UnderwritingAttachmentMappingProfile : Profile
    {
        public UnderwritingAttachmentMappingProfile()
        {
            CreateMap<UnderwritingAttachment, UnderwritingAttachmentDto>();
        }
    }
}