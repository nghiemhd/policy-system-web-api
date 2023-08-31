using AutoMapper;
using SingLife.ULTracker.UseCases.Common.Applications;
using SingLife.ULTracker.WebAPI.Contracts.Common.Applications;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class ApplicationMappingsProfile : Profile
    {
        public ApplicationMappingsProfile()
        {
            CreateMap<SearchApplicationsResultDto, SearchApplicationResult>();

            CreateMap<MatchedApplicationDto, MatchedApplication>();

            CreateMap<SearchApplicationRequest, SearchApplicationsQuery>();

            CreateMap<LifeAssuredDto, LifeAssured>();

            CreateMap<ContactDetailsDto, ContactDetails>();

            CreateMap<PolicyOwnerDto, PolicyOwner>();

            CreateMap<PayorDto, Payor>();

            CreateMap<OtherCustomerDto, OtherCustomer>();

            CreateMap<AuthorisedPersonDto, AuthorisedPerson>();

            CreateMap<OrganisationDto, Organisation>();

            CreateMap<OrganisationPayorDto, OrganisationPayor>();

            CreateMap<OtherOrganisationDto, OtherOrganisation>();
        }
    }
}