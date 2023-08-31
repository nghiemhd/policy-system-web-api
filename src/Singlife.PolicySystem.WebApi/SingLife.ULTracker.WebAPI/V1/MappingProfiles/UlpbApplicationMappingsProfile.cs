using AutoMapper;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Applications;
using Dtos = SingLife.ULTracker.UseCases.Ulpb.V1.Applications.DTOs;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UlpbApplicationMappingsProfile : Profile
    {
        public UlpbApplicationMappingsProfile()
        {
            CreateMap<Dtos.GeneralInformationDto, GeneralInformation>();

            CreateMap<Dtos.ApplicationDetailsDto, ApplicationDetails>();

            CreateMap<Dtos.UnderwritingDetailsDto, LifeAssuredUnderwriting>();

            CreateMap<Dtos.PaymentDetailsDto, PaymentDetails>();

            CreateMap<Dtos.ApplicationTermsOfOfferDto, ApplicationTermsOfOffer>();

            CreateMap<Dtos.ApplicationDespatchDto, ApplicationDespatch>();

            CreateMap<Dtos.FIPurposeOfInsuranceDto, FIPurposeOfInsurance>();

            CreateMap<Dtos.FIIncomeDto, FIIncome>();

            CreateMap<Dtos.FIAssetDto, FIAsset>();

            CreateMap<Dtos.FILiabilityDto, FILiability>();

            CreateMap<Dtos.FIPropertyDto, FIProperty>();

            CreateMap<Dtos.FinancialInfoDto, FinancialInfo>();

            CreateMap<Dtos.OtherInsuranceDto, OtherInsurance>();

            CreateMap<Dtos.QueriedUlpbApplicationDto, UlpbApplicationDetails>();
        }
    }
}