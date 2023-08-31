using AutoMapper;
using SingLife.PolicySystem.UA.UseCases.Policies;
using SingLife.PolicySystem.UA.UseCases.Policies.Covers;
using SingLife.PolicySystem.UA.UseCases.Transactions;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.UA.Checklist;
using SingLife.ULTracker.WebAPI.Contracts.UA.Customers;
using SingLife.ULTracker.WebAPI.Contracts.UA.Policies;
using SingLife.ULTracker.WebAPI.Contracts.UA.Policies.Covers;
using SingLife.ULTracker.WebAPI.Contracts.UA.Transactions;

namespace SingLife.PolicySystem.UA.WebApi.V1.MappingProfiles
{
    public class UAPolicyMappingsProfile : Profile
    {
        public UAPolicyMappingsProfile()
        {
            CreateMap<Page<MatchedTransactionDto>, PagedSearchResult<Transaction>>();

            CreateMap<UAPolicySummaryDto, UAPolicySummary>()
                .ForMember(dest => dest.CancellationReason, opt => opt.Ignore())
                .ForMember(dest => dest.CancellationDate, opt => opt.Ignore());

            CreateMap<UAPolicyDetailsDto, UAPolicyDetails>()
                .ForMember(dest => dest.ApplicationStatus, opt => opt.Ignore())
                .ForMember(dest => dest.RateLockRates, opt => opt.Ignore());

            CreateMap<GeneralInformationDto, GeneralInformation>();

            CreateMap<PolicyTermsOfOfferDTO, PolicyTermsOfOffer>();

            CreateMap<PolicyOwnerDto, PolicyOwner>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<TransactionDto, Transaction>()
                .ForMember(dest => dest.CanBeModified, opt => opt.Ignore());

            CreateMap<ChecklistDto, Checklist>();

            CreateMap<ChecklistItemDto, ChecklistItem>();

            CreateMap<PolicyTermsOfOfferDto, PolicyTermsOfOffer>()
                .ForMember(dest => dest.PrelimDateOfOffer, opt => opt.Ignore())
                .ForMember(dest => dest.ApplicationDateOfOffer, opt => opt.Ignore())
                .ForMember(dest => dest.DateOfTOAExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.RevisedDateOfOffer, opt => opt.Ignore())
                .ForMember(dest => dest.Comment, opt => opt.Ignore());

            CreateMap<RateLockRatesDto, RateLockRates>();

            CreateMap<TrancheRateDto, TrancheRate>();

            CreateMap<UnemploymentBenefitCoverDto, UnemploymentBenefitCover>();

            CreateMap<UnemploymentBenefitClaimDto, UnemploymentBenefitClaim>();

            CreateMap<EditPolicyRequest, EditPolicyCommand>()
                .ForMember(dest => dest.TermsOfOffer, opt => opt.MapFrom(src => src.PolicyTermsOfOffer))
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore());

            CreateMap<SearchPolicyTransactionsRequest, SearchPolicyTransactionsQuery>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => $"{src.OrderBy},{src.OrderDirection}"))
                .ForMember(dest => dest.PageIndex, opt => opt.MapFrom(src => src.CurrentPage - 1));

            CreateMap<Page<MatchedTransactionDto>, PagedSearchResult<MatchedTransaction>>();

            CreateMap<TransactionDto, MatchedTransaction>()
                .ForMember(dest => dest.CanBeModified, opt => opt.Ignore());
        }
    }
}