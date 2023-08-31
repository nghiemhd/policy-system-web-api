using AutoMapper;
using SingLife.ULTracker.WebAPI.Contracts.Quotations;
using ULQuotationCalculatePremiumRequest = SingLife.UniversalLifeQuotation.WebAPI.Contracts.V1.Quotations.CalculatePremiumRequest;
using ULQuotationLifeAssuredDetails = SingLife.UniversalLifeQuotation.WebAPI.Contracts.V1.Quotations.LifeAssuredDetails;
using ULQuotationQuotationError = SingLife.UniversalLifeQuotation.WebAPI.Contracts.V1.Quotations.QuotationError;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class ULQuotationMappingsProfile : Profile
    {
        public ULQuotationMappingsProfile()
        {
            CreateMap<CalculatePremiumRequest, ULQuotationCalculatePremiumRequest>();
            CreateMap<CalculatePremiumRequest.PlanSetupData, ULQuotationCalculatePremiumRequest.PlanSetupData>();
            CreateMap<CalculatePremiumRequest.LumpSumPremiumData, ULQuotationCalculatePremiumRequest.LumpSumPremiumData>();
            CreateMap<CalculatePremiumRequest.SplitPremiumData, ULQuotationCalculatePremiumRequest.SplitPremiumData>();
            CreateMap<CalculatePremiumRequest.TopUpPremiumData, ULQuotationCalculatePremiumRequest.TopUpPremiumData>();

            CreateMap<QuotationLifeAssuredDetails, ULQuotationLifeAssuredDetails>();

            CreateMap<ULQuotationQuotationError, QuotationError>();
        }
    }
}