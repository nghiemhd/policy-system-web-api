using AutoMapper;
using SingLife.ULTracker.UseCases.Vul.V1.Documents;
using SingLife.ULTracker.UseCases.VulPolicies;
using SingLife.ULTracker.WebAPI.Contracts.VulPolicies;
using QuotationEngineContracts = SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class VulPolicyMappingProfile : Profile
    {
        public VulPolicyMappingProfile()
        {
            CreateMap<VulPolicyDTO, VulPolicy>()
                .ForMember(dest => dest.IsJointPolicyOwner, opt => opt.MapFrom(src => src.PolicyOwner2Details != null))
                .ForMember(dest => dest.FinancialInfo, opt => opt.Ignore())
                .ForMember(dest => dest.OtherInsurances, opt => opt.Ignore());

            CreateMap<VulPolicy, VulPolicyDTO>()
                .ForMember(dest => dest.DeclinedReason, opt => opt.Ignore())
                .ForMember(dest => dest.DateOfDecline, opt => opt.Ignore())
                .ForMember(dest => dest.UnderwritingRequirements, opt => opt.Ignore())
                .ForMember(dest => dest.FirstPlannedPremium, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerEmails, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerFullName, opt => opt.Ignore());

            CreateMap<PaymentDetailsDTO, PaymentDetails>().ReverseMap();
            CreateMap<AssetManagerDetailsDTO, AssetManagerDetails>().ReverseMap();
            CreateMap<BankDetailsDTO, BankDetails>().ReverseMap();

            CreateMap<PIDocumentDataDto, QuotationEngineContracts.Quotes.Vul.GenerateBIDocumentRequest>()
                .ForMember(dest => dest.ClientDetails, opt => opt.MapFrom(src => src.ClientDetails))
                .ForMember(dest => dest.PlanDetails, opt => opt.MapFrom(src => src.PlanDetails));
            CreateMap<ClientDetailsDto, QuotationEngineContracts.Quotes.Vul.ClientDetailsInput>();
            CreateMap<LifeAssuredDto, QuotationEngineContracts.Common.LifeAssured>();
            CreateMap<PlanDetailsDto, QuotationEngineContracts.Quotes.Vul.PlanDetailsInput>()
                .ForMember(dest => dest.DeathBenefitOption, opt => opt.MapFrom(src => src.BenefitType));
        }
    }
}