using AutoMapper;
using SingLife.ULTracker.UseCases.Customers;
using SingLife.ULTracker.WebAPI.Contracts.Customers;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class OrganisationMappingProfiles : Profile
    {
        public OrganisationMappingProfiles()
        {
            CreateMap<CreateOtherOrganisationRequest, CreateOtherOrganisationCommand>()
                .ForMember(dest => dest.OrganisationId, opt => opt.Ignore());

            CreateMap<CreateOtherOrganisationRequest, CreateOrganisationCommand>();

            CreateMap<EditOtherOrganisationRequest, EditOtherOrganisationPartiallyCommand>();

            CreateMap<EditOtherOrganisationRequest, EditOtherOrganisationCommand>();
        }
    }
}