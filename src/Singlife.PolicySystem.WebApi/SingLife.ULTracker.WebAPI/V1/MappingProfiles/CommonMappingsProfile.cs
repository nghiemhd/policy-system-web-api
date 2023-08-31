using AutoMapper;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.PolicyAccountValues;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Common.Policies;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class CommonMappingsProfile : Profile
    {
        public CommonMappingsProfile()
        {
            CreateMap<AddressDto, Address>().ReverseMap();

            CreateMap<FileDto, FileContract>().ReverseMap();

            CreateMap<SaveSpecialQuoteFactorsRequest, SaveSpecialQuoteFactorsCommand>()
                .ForMember(dest => dest.PremiumCharge, opt => opt.ResolveUsing(src => src.PremiumCharge * 0.01M))
                .ForMember(dest => dest.CoiChargeAdjustment, opt => opt.ResolveUsing(src => src.CoiChargeAdjustment * 0.01M))
                .ForMember(dest => dest.MarketingAllowance, opt => opt.ResolveUsing(src => src.MarketingAllowance * 0.01M))
                .ForMember(dest => dest.OverfundCharge, opt => opt.ResolveUsing(src => src.OverfundCharge * 0.01M));

            CreateMap<PolicyTransactionRate, PolicyTransactionRateDto>()
                .ForMember(dest => dest.GAPPRate, opt => opt.MapFrom(src => src.GAPPRate * 0.01M))
                .ForMember(dest => dest.IAOPRate, opt => opt.MapFrom(src => src.IAOPRate * 0.01M))
                .ForMember(dest => dest.GAOPRate, opt => opt.MapFrom(src => src.GAOPRate * 0.01M));

            CreateMap<GetAccountValuesRecalculationRecordRequest, GetAccountValuesRecalculationRecordsQuery>()
                .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.CurrentPage));

            CreateMap<RecalculateAccountValuesRequest, RecalculateAccountValuesWithSpecificDateCommand>()
                .ForMember(dest => dest.Product, opt => opt.Ignore());

            CreateMap<ProductVersionDto, ProductVersion>().ReverseMap();

            CreateMap<GetAccountValuesRecalculationRecordResult, AccountValuesRecalculationRecordResult>();
        }
    }
}