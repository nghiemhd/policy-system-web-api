using AutoMapper;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.Policies;
using SingLife.ULTracker.UseCases.UL.V1.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Policies;
using GeneralInformationContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.GeneralInformation;
using ClaimContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.Claim;
using LifeAssuredUnderwritingContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.LifeAssuredUnderwriting;
using PlanDetailContract = SingLife.ULTracker.WebAPI.Contracts.Policies.PlanDetail;
using PolicyDespatchContract = SingLife.ULTracker.WebAPI.Contracts.Common.Policies.PolicyDespatch;
using QuotationEngineContracts = SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class PolicyMappingsProfile : Profile
    {
        public PolicyMappingsProfile()
        {
            CreateMap<QueriedPolicyDto, Policy>()
                .ForMember(dest => dest.IsJointPolicyOwner, opt => opt.Ignore());

            CreateMap<Contracts.Customers.PayorOrganisationDetails, UseCases.Customers.OrganisationSnapshotDTO>()
                .ForMember(dest => dest.OrganisationId, opt => opt.Ignore())
                .ForMember(dest => dest.RegisteredType, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceType, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentType, opt => opt.Ignore());

            CreateMap<Policy, PolicyDTO>()
                .ForMember(dest => dest.AssigneeName, opt => opt.MapFrom(src => src.AssigneeName))
                .ForMember(dest => dest.AssignedUnderwriterAndCaseManagerId, opt => opt.MapFrom(src => src.AssignedUnderwriterAndCaseManager.Id))
                .ForMember(dest => dest.OwnershipType, opt => opt.Ignore())
                .ForMember(dest => dest.UnderwritingRequirements, opt => opt.Ignore())
                .ForMember(dest => dest.FirstPlannedPremium, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerEmails, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerFullName, opt => opt.Ignore());

            CreateMap<GeneralInformationDTO, GeneralInformationContract>().ReverseMap();
            CreateMap<ClaimDto, ClaimContract>().ReverseMap();
            CreateMap<PlanDetailsDTO, PlanDetailContract>().ReverseMap();
            CreateMap<PaymentDetailsDTO, PaymentDetail>()
                .ForMember(dest => dest.FirstYearPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedPlannedPremiumPaymentOption, opt => opt.Ignore());

            CreateMap<TopUpPlannedPremium, TopUpPlannedPremiumDto>();
            CreateMap<PaymentDetail, PaymentDetailsDTO>();
            CreateMap<LifeAssuredUnderwritingDTO, LifeAssuredUnderwritingContract>().ReverseMap();
            CreateMap<PolicyTermsOfOfferDTO, TermsOfOffer>().ReverseMap();
            CreateMap<PolicyDespatchDTO, PolicyDespatchContract>().ReverseMap();

            CreateMap<UseCases.Common.Policies.SearchPoliciesResult, Contracts.Policies.SearchPoliciesResult>()
                .ForMember(dest => dest.Policies, opt => opt.MapFrom(src => src.PoliciesList));

            CreateMap<SearchPolicyDTO, MatchedPolicy>();

            CreateMap<ExportPoliciesRequest, GetPoliciesForExportQuery>();

            CreateMap<SpecialQuoteFactorsDto, SpecialQuoteFactors>()
                .ForMember(dest => dest.PremiumCharge, opt => opt.ResolveUsing(src => src.PremiumCharge * 100M))
                .ForMember(dest => dest.CoiChargeAdjustment, opt => opt.ResolveUsing(src => src.CoiChargeAdjustment * 100M))
                .ForMember(dest => dest.OverfundCharge, opt => opt.ResolveUsing(src => src.OverfundCharge * 100M))
                .ForMember(dest => dest.MarketingAllowance, opt => opt.ResolveUsing(src => src.MarketingAllowance * 100M))
                .ForMember(dest => dest.TransactionInterestRates, opt => opt.Ignore());

            CreateMap<PolicyTransactionRateDto, PolicyTransactionRate>()
                .ForMember(dest => dest.GAPPRate, opt => opt.MapFrom(src => src.GAPPRate * 100M))
                .ForMember(dest => dest.IAOPRate, opt => opt.MapFrom(src => src.IAOPRate * 100M))
                .ForMember(dest => dest.GAOPRate, opt => opt.MapFrom(src => src.GAOPRate * 100M));

            CreateMap<PolicyChargesDocumentDataDto, QuotationEngineContracts.Quotes.UL.GenerateChargesDocumentRequest>()
                .ForMember(dest => dest.LumpsumPremium, opt => opt.MapFrom(src => src.LumpSumPremium));

            CreateMap<ClientDetailsDto, QuotationEngineContracts.Quotes.UL.ClientDetailsInput>()
                .ForMember(dest => dest.RefNumber, opt => opt.Ignore());

            CreateMap<LifeAssuredDto, QuotationEngineContracts.Common.LifeAssured>();

            CreateMap<UseCases.Common.AddressDto, QuotationEngineContracts.Common.Address>();

            CreateMap<LumpSumPremiumDto, QuotationEngineContracts.Quotes.UL.ChargesLumpsumPremiumInput>()
                .ForMember(dest => dest.Premium, opt => opt.Ignore());

            CreateMap<SplitPremiumDto, QuotationEngineContracts.Quotes.UL.ChargesSplitPremiumInput>();

            CreateMap<TopUpPremiumDto, QuotationEngineContracts.Quotes.UL.ChargesTopUpPremiumInput>()
                .ForMember(dest => dest.MinFirstYearPremiumPercent, opt => opt.Ignore());

            CreateMap<TopupPremiumPercentDto, QuotationEngineContracts.Quotes.UL.TopupPremiumPercent>();

            CreateMap<PIDocumentDataDto, QuotationEngineContracts.Quotes.UL.GenerateBIDocumentRequest>();

            CreateMap<LumpSumPremiumDto, QuotationEngineContracts.Quotes.UL.LumpsumPremiumInput>()
                .ForMember(dest => dest.Premium, opt => opt.Ignore());

            CreateMap<SplitPremiumDto, QuotationEngineContracts.Quotes.UL.SplitPremiumInput>();

            CreateMap<TopUpPremiumDto, QuotationEngineContracts.Quotes.UL.TopUpPremiumInput>()
                 .ForMember(dest => dest.MinFirstYearPremiumPercent, opt => opt.Ignore());

            CreateMap<UseCases.Common.Policies.PolicyIdentity, Contracts.Common.Policies.PolicyIdentity>();
        }
    }
}