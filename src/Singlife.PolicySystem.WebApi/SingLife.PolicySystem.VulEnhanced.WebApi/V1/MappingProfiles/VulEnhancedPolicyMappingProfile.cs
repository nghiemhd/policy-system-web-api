using AutoMapper;
using SingLife.PolicySystem.VulEnhanced.UseCases.Policies;
using SingLife.ULTracker.WebAPI.Contracts.VulEnhanced.Policies;

namespace SingLife.PolicySystem.VulEnhanced.WebApi.V1.MappingProfiles
{
    public class VulEnhancedPolicyMappingProfile : Profile
    {
        public VulEnhancedPolicyMappingProfile()
        {
            CreateMap<CreatePolicyRequest, CreatePolicyCommand>()
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore());

            CreateMap<EditPolicyRequest, EditPolicyCommand>()
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore())
                .ForMember(dest => dest.IsUpdatingPolicyVersion, opt => opt.Ignore());

            CreateMap<PolicyDetails, PolicyDetailsDto>().ReverseMap();
            CreateMap<PaymentDetails, PaymentDetailsDto>().ReverseMap();
            CreateMap<AssetManagerDetails, AssetManagerDetailsDto>().ReverseMap();
            CreateMap<BankDetails, BankDetailsDto>();
            CreateMap<ContactPerson, ContactPersonDto>();
            CreateMap<FinancialInfo, FinancialInfoDto>()
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore());

            CreateMap<FIPurposeOfInsurance, FIPurposeOfInsuranceDto>()
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FIIncome, FIIncomeDto>()
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FIAsset, FIAssetDto>()
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FILiability, FILiabilityDto>()
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FIProperty, FIPropertyDto>()
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<OtherInsurance, OtherInsuranceDto>()
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore());

            CreateMap<VulEnhancedPolicyDto, VulEnhancedPolicy>()
                .ForMember(dest => dest.IsJointPolicyOwner, opt => opt.MapFrom(src => src.PolicyOwner2Details != null));
        }
    }
}