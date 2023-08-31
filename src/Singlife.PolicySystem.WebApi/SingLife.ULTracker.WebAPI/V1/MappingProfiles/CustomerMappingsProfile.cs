using AutoMapper;
using SingLife.ULTracker.Model.Common;
using SingLife.ULTracker.UseCases.Auth;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.Common.Customers;
using SingLife.ULTracker.UseCases.Customers;
using SingLife.ULTracker.UseCases.Customers.DataExports;
using SingLife.ULTracker.UseCases.Ulpb.V1.CustomerNotification;
using SingLife.ULTracker.WebAPI.Contracts.Customers;
using SingLife.ULTracker.WebAPI.Contracts.Notification;
using SingLife.ULTracker.WebAPI.Contracts.Roles;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class CustomerMappingsProfile : Profile
    {
        public CustomerMappingsProfile()
        {
            CreateMap<OrganisationSnapshotDTO, PayorOrganisationDetails>()
                .ForMember(dest => dest.PayorRelationship, opt => opt.Ignore());

            CreateMap<CustomerDetailsDto, CustomerDetails>();

            CreateMap<ApplicationSummaryDto, ApplicationSummary>()
                .ForMember(dest => dest.PolicyOwnerName, opt => opt.Ignore());

            CreateMap<CreateOtherCustomerRequest, CreateOtherCustomerCommand>()
                .AfterMap((_, dest) =>
                {
                    SetWhenNotNull(dest.ResidentialAddress, AddressType.Residential);
                    SetWhenNotNull(dest.CorrespondenceAddress, AddressType.Correspondence);
                    SetWhenNotNull(dest.ForeignPermanentAddress, AddressType.ForeignPermanent);
                    SetWhenNotNull(dest.BusinessAddress, AddressType.Business);
                });

            CreateMap<EditOtherCustomerRequest, EditOtherCustomerCommand>()
                .ForMember(dest => dest.RelationshipType, opt => opt.ResolveUsing(src => src.EditAllFields ? src.RelationshipType : Option<string>.None()))
                .ForMember(dest => dest.IdType, opt => opt.ResolveUsing(src => src.EditAllFields ? src.IdType : Option<string>.None()))
                .ForMember(dest => dest.IdNumber, opt => opt.ResolveUsing(src => src.EditAllFields ? src.IdNumber : Option<string>.None()))
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore())
                .AfterMap((_, dest) =>
                {
                    SetWhenNotNull(dest.ResidentialAddress, AddressType.Residential);
                    SetWhenNotNull(dest.CorrespondenceAddress, AddressType.Correspondence);
                    SetWhenNotNull(dest.ForeignPermanentAddress, AddressType.ForeignPermanent);
                    SetWhenNotNull(dest.BusinessAddress, AddressType.Business);
                });

            CreateMap<DeleteOtherCustomerRequest, DeleteCustomerSnapshotCommand>()
                .ForMember(dest => dest.CustomerSnapshotId, opt => opt.MapFrom(src => src.OtherCustomerId));

            CreateMap<DeleteOtherOrganisationRequest, DeleteOrganisationSnapshotCommand>()
                .ForMember(dest => dest.OrganisationSnapshotId, opt => opt.MapFrom(src => src.OtherOrganizationId));

            CreateMap<EditCustomerRequest, UpdateCustomerCommand>()
                .ForMember(dest => dest.UniqueCorrelationId, opt => opt.Ignore())
                .ForMember(dest => dest.AuthenticatedUser, opt => opt.Ignore());

            CreateMap<SearchCustomerRequest, SearchCustomersQuery>();

            CreateMap<SimplifiedCustomer, CustomerSummary>();

            CreateMap<CustomerPage, SearchCustomerResult>();

            CreateMap<PolicySummaryDto, PolicySummary>();

            CreateMap<OrganisationDetailsDto, OrganisationDetails>().ReverseMap();

            CreateMap<Customer, CustomerSnapshotDTO>()
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.PolicyId, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialType, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceType, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentType, opt => opt.Ignore())
                .ForMember(dest => dest.ContactDetails, opt => opt.Ignore());

            CreateMap<PayorDetailsDto, PayorDetails>()
                .ForMember(dest => dest.RelationshipToPolicyOwner, opt => opt.Ignore())
                .ForMember(dest => dest.PayorRelationship, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.PreferredName, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.Ignore())
                .ForMember(dest => dest.Salutation, opt => opt.Ignore())
                .ForMember(dest => dest.IdType, opt => opt.Ignore())
                .ForMember(dest => dest.IdExpiry, opt => opt.Ignore())
                .ForMember(dest => dest.Nationality, opt => opt.Ignore())
                .ForMember(dest => dest.Gender, opt => opt.Ignore())
                .ForMember(dest => dest.MaritalStatus, opt => opt.Ignore())
                .ForMember(dest => dest.DateOfBirth, opt => opt.Ignore())
                .ForMember(dest => dest.Age, opt => opt.Ignore())
                .ForMember(dest => dest.ResidencyStatus, opt => opt.Ignore())
                .ForMember(dest => dest.CountryOfResidence, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialAddress1, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialAddress2, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialAddress3, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialAddress4, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialCountry, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialPostalCode, opt => opt.Ignore())
                .ForMember(dest => dest.ResidentialState, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceAddress1, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceAddress2, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceAddress3, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceAddress4, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceCountry, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondencePostalCode, opt => opt.Ignore())
                .ForMember(dest => dest.CorrespondenceState, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentAddress1, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentAddress2, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentAddress3, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentAddress4, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentCountry, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentPostalCode, opt => opt.Ignore())
                .ForMember(dest => dest.ForeignPermanentState, opt => opt.Ignore())
                .ForMember(dest => dest.AnnualIncome, opt => opt.Ignore())
                .ForMember(dest => dest.AnnualIncomeCurrency, opt => opt.Ignore())
                .ForMember(dest => dest.Occupation, opt => opt.Ignore())
                .ForMember(dest => dest.NameOfEmployer, opt => opt.Ignore())
                .ForMember(dest => dest.BusinessAddress, opt => opt.Ignore())
                .ForMember(dest => dest.NatureOfBusiness, opt => opt.Ignore())
                .ForMember(dest => dest.ExactDutiesWithDetails, opt => opt.Ignore())
                .ForMember(dest => dest.TaxpayerIdNumber, opt => opt.Ignore())
                .ForMember(dest => dest.SourceOfFunds, opt => opt.Ignore())
                .ForMember(dest => dest.PepStatus, opt => opt.Ignore())
                .ForMember(dest => dest.PoliticallyExposedDetails, opt => opt.Ignore());
            CreateMap<LifeAssuredSnapshotDTO, LifeAssuredDetails>().ReverseMap();
            CreateMap<OrganisationSnapshotDTO, Organisation>().ReverseMap();
            CreateMap<PolicyOwnerSnapshotDTO, PolicyOwnerDetails>().ReverseMap();
            CreateMap<ContactDetailsDto, ContactDetails>().ReverseMap();
            CreateMap<PayorSnapshotDto, PayorDetails>()
                .ForMember(dest => dest.AssigneeName, opt => opt.Ignore())
                .ForMember(dest => dest.PayorRelationship, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<ExportCustomersRequest, GetExportCustomersDataQuery>();

            CreateMap<AuthorisedPersonDto, AuthorisedPerson>();

            CreateMap<TaxResidenceDto, TaxResidence>();

            CreateMap<EditCustomerIdentityRequest, UpdateCustomerIdentityCommand>();

            CreateMap<EditUserRolesRequest, EditUserRolesCommand>();

            CreateMap<SendEmailToCustomerResponse, SendingEmailResponse>();
        }

        private void SetWhenNotNull(AddressDto address, string addressType)
        {
            if (address != null)
                address.Type = addressType;
        }
    }
}