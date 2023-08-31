using AutoMapper;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.UseCases.Ulpb.V1.Documents;
using SingLife.ULTracker.UseCases.Ulpb.V1.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Customers;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Documents;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Policies;
using System;
using PlanDetailContract = SingLife.ULTracker.WebAPI.Contracts.Policies.PlanDetail;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UlpbPolicyMappingsProfile : Profile
    {
        public UlpbPolicyMappingsProfile()
        {
            CreateMap<QueriedUlpbPolicyDto, UlpbPolicy>()
                .ForMember(dest => dest.ApplicationChecklist, opt => opt.Ignore())
                .ForMember(dest => dest.IsJointPolicyOwner, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerDetails, opt => opt.ResolveUsing((src, dest, poDetails, ctx) => ResolvePolicyOwnerDetails(src, ctx)))
                .ForMember(dest => dest.IsIndividualPolicyOwner, opt => opt.ResolveUsing(src => src.OrganisationDetails == null));

            CreateMap<UlpbPolicySummaryDto, UlpbPolicySummary>()
                .ForMember(dest => dest.IsJointPolicyOwner, opt => opt.Ignore())
                .ForMember(dest => dest.LifeAssured1UWDetails, opt => opt.Ignore())
                .ForMember(dest => dest.LifeAssured2UWDetails, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedUnderwriterAndCaseManager, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialInfo, opt => opt.Ignore())
                .ForMember(dest => dest.OtherInsurances, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerDetails, opt => opt.ResolveUsing((src, dest, poDetails, ctx) => ResolvePolicyOwnerDetails(src, ctx)))
                .ForMember(dest => dest.IsIndividualPolicyOwner, opt => opt.ResolveUsing(src => src.OrganisationDetails == null));

            CreateMap<UlpbPolicy, UlpbPolicyDTO>()
                .ForMember(dest => dest.OwnershipType, opt => opt.Ignore())
                .ForMember(dest => dest.UnderwritingRequirements, opt => opt.Ignore())
                .ForMember(dest => dest.FirstPlannedPremium, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerEmails, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyOwnerFullName, opt => opt.Ignore());

            CreateMap<PlanDetailsDTO, PlanDetailContract>()
                .ForMember(dest => dest.Honeymoon, opt => opt.Ignore());

            CreateMap<PlanDetailContract, PlanDetailsDTO>();

            CreateMap<PaymentDetailsDTO, PaymentDetail>()
                .ForMember(dest => dest.FirstYearPercentage, opt => opt.Ignore())
                .ForMember(dest => dest.SelectedPlannedPremiumPaymentOption, opt => opt.Ignore());

            CreateMap<PaymentDetail, PaymentDetailsDTO>();

            CreateMap<CreatePolicyRequest, CreateUlpbPolicyCommand>()
                .ForMember(dest => dest.PayorOrganisationSnapshotDetails, opt => opt.MapFrom(src => src.PayorOrganisationDetails))
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore())
                .ForMember(dest => dest.FirstPlannedPremium, opt => opt.Ignore());

            CreateMap<EditPolicyRequest, EditUlpbPolicyCommand>()
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore())
                .ForMember(dest => dest.FirstPlannedPremium, opt => opt.Ignore());

            CreateMap<OtherInsurance, OtherInsuranceDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore());

            CreateMap<PrintWelcomeLetterAndSchedulesDocumentsRequest, GetWelcomeLetterAndSchedulesDocumentDataQuery>();

            CreateFinancialInfoMappingsProfile();
        }

        private void CreateFinancialInfoMappingsProfile()
        {
            CreateMap<FinancialInfo, FinancialInfoDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore());

            CreateMap<FIPurposeOfInsurance, FIPurposeOfInsuranceDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FIIncome, FIIncomeDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FIAsset, FIAssetDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FILiability, FILiabilityDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());

            CreateMap<FIProperty, FIPropertyDto>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialInfoId, opt => opt.Ignore());
        }

        private PolicyOwnerDetails ResolvePolicyOwnerDetails(BasePolicyDTO src, ResolutionContext ctx)
        {
            if (src.OrganisationDetails != null)
                return null;

            return src.PolicyOwnerDetails != null
                ? ctx.Mapper.Map<PolicyOwnerDetails>(src.PolicyOwnerDetails)
                : CreateEmptyOwner();
        }

        private PolicyOwnerDetails CreateEmptyOwner()
        {
            var policyOwnerDetails = new PolicyOwnerDetails
            {
                DateOfBirth = DateTime.MinValue,
                ContactDetails = new ContactDetails(),
                BusinessAddress = new Contracts.Common.Address()
            };

            return policyOwnerDetails;
        }
    }
}