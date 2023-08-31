using AutoMapper;
using SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.ULPB;
using SingLife.PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.ULPB.InforceIllustration;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Ulpb.V1.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Policies;
using SingLife.ULTracker.WebAPI.Contracts.UlpbPolicies.Documents;
using TermOfAcceptanceDocumentContract = SingLife.ULTracker.WebAPI.Contracts.UlpbPolicies.Documents.TermOfAcceptanceDocument;
using TermOfAcceptanceDocumentDto = SingLife.ULTracker.UseCases.Ulpb.V1.Documents.TermOfAcceptanceDocument;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UlpbPolicyDocumentMappingsProfile : Profile
    {
        public UlpbPolicyDocumentMappingsProfile()
        {
            CreateMap<TermOfAcceptanceDocumentDto, TermOfAcceptanceDocumentContract>();

            CreateMap<PrintUlpbPolicyTermOfAcceptanceRequest, GetTermOfAcceptanceDocumentDataQuery>()
                .ForMember(x => x.PolicyId, opt => opt.MapFrom(x => x.PolicyId));

            CreateMap<PrintUlpbPolicyStatementDocumentRequest, GetPolicyStatementDocumentDataQuery>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.From))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.To));

            CreateMap<PrintUlpbCommissionStatementDocumentRequest, GetCommissionStatementDocumentDataQuery>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(x => x.From))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(x => x.To));

            CreateMap<AddressDto, PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.ULPB.InforceIllustration.Address>();
            CreateMap<AddressDto, PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.ULPB.Address>();
            CreateMap<InforceIllustrationDocumentLifeAssured, PolicySystem.QuotationEngine.WebApi.Contracts.V1.Quotes.ULPB.InforceIllustration.LifeAssured>();

            CreateMappingForLumpSumPremium();
            CreateMappingForTopUpPremium();
        }

        private void CreateMappingForLumpSumPremium()
        {
            CreateMap<FinalPIDocumentDataDto, GenerateLumpsumPIDocumentRequest>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.PolicyOwnerName))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.VersionOfProduct))
                .ForMember(dest => dest.LumpsumPremium, opt => opt.MapFrom(src => src.LumpSumPremium));

            CreateMap<FinalPIDocumentClientDetailsDto, ClientDetailsPIDocumentInput>()
                .ForMember(contract => contract.LifeAssured1, mce => mce.MapFrom(dto => dto.FirstLifeAssured))
                .ForMember(dest => dest.RefNumber, opt => opt.Ignore());

            CreateMap<FinalPIDocumentLumpSumPremiumDto, LumpsumPremiumInput>();

            CreateMap<InforceIllustrationDocumentLumpSumPremiumDto, LumpsumPremiumInput>()
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.RateLockRate, opt => opt.Ignore());

            CreateMap<InforceIllustrationDocumentDataDto, GenerateLumpSumInforceIllustrationDocumentRequest>()
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.QuoteType, opt => opt.Ignore())
                .ForMember(dest => dest.FundingOption, opt => opt.Ignore())
                .ForMember(dest => dest.AdviserFullName, opt => opt.Ignore())
                .ForMember(dest => dest.AdviserGender, opt => opt.Ignore())
                .ForMember(dest => dest.InitialPremiumTransactionValuationDate, opt => opt.Ignore())
                .ForMember(dest => dest.InitialPremiumTransactionDate, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.LifeAssured1, opt => opt.Ignore())
                .ForMember(dest => dest.LumpsumPremium, opt => opt.Ignore())
                .ForMember(dest => dest.PremiumPaidDates, opt => opt.Ignore());
        }

        private void CreateMappingForTopUpPremium()
        {
            CreateMap<FinalPIDocumentDataDto, GenerateTopUpPIDocumentRequest>()
                .ForMember(dest => dest.OwnerName, opt => opt.MapFrom(src => src.PolicyOwnerName))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.VersionOfProduct));

            CreateMap<FinalPIDocumentTopUpPremiumDto, TopUpPremiumInput>()
                .ForMember(dest => dest.MinFirstYearPremiumPercent, opt => opt.Ignore());

            CreateMap<InforceIllustrationDocumentTopUpPremiumDto, InforceIllustrationTopUpPremiumInput>()
                .ForMember(dest => dest.MinFirstYearPremiumPercent, opt => opt.Ignore());

            CreateMap<FinalPIDocumentTopupPremiumPercent, TopupPremiumPercent>();

            CreateMap<InforceIllustrationDocumentTopUpPremiumDto, TopUpPremiumInput>()
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.MinFirstYearPremiumPercent, opt => opt.Ignore())
                .ForMember(dest => dest.RateLockRate, opt => opt.Ignore());

            CreateMap<InforceIllustrationDocumentDataDto, GenerateTopUpInforceIllustrationDocumentRequest>()
                .ForMember(dest => dest.Version, opt => opt.Ignore())
                .ForMember(dest => dest.QuoteType, opt => opt.Ignore())
                .ForMember(dest => dest.FundingOption, opt => opt.Ignore())
                .ForMember(dest => dest.AdviserFullName, opt => opt.Ignore())
                .ForMember(dest => dest.AdviserGender, opt => opt.Ignore())
                .ForMember(dest => dest.InitialPremiumTransactionValuationDate, opt => opt.Ignore())
                .ForMember(dest => dest.InitialPremiumTransactionDate, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.LifeAssured1, opt => opt.Ignore())
                .ForMember(dest => dest.TopUpPremium, opt => opt.Ignore())
                .ForMember(dest => dest.PremiumPaidDates, opt => opt.Ignore());
        }
    }
}