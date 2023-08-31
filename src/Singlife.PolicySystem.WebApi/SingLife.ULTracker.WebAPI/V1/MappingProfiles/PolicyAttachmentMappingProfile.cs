using AutoMapper;
using SingLife.ULTracker.UseCases.PolicyAttachments;
using SingLife.ULTracker.WebAPI.Contracts.PolicyAttachments;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class PolicyAttachmentMappingProfile : Profile
    {
        public PolicyAttachmentMappingProfile()
        {
            CreateMap<PolicyAttachment, PolicyAttachmentDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UploadedOnUtc, opt => opt.Ignore());

            CreateMap<PolicyRelatedAttachmentsDto, PolicyRelatedAttachments>();

            CreateMap<PolicyAttachmentSummaryDto, PolicyAttachmentSummary>()
                .ForMember(dest => dest.UploadedAtUtc, opt => opt.MapFrom(src => src.UploadedOnUtc));

            CreateMap<TransactionAttachmentSummaryDto, TransactionAttachmentSummary>()
                .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => TransactionMappingsProfile.ConvertFromEnumToString(src.TransactionType)));

            CreateMap<UnderwritingAttachmentSummaryDto, UnderwritingAttachmentSummary>();
        }
    }
}