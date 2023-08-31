using AutoMapper;
using SingLife.ULTracker.UseCases.Auth;
using SingLife.ULTracker.UseCases.Auth.DTOs;
using SingLife.ULTracker.WebAPI.Contracts.Roles;
using SingLife.ULTracker.WebAPI.Contracts.Users;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class UserMappingsProfile : Profile
    {
        public UserMappingsProfile()
        {
            CreateMap<AssignRolesToUserRequest, AssignRolesToUserCommand>();

            CreateMap<DeleteUserRequest, DeleteUserCommand>();

            CreateMap<UserDto, User>();

            CreateMap<UserDto, UserQueriedByRole>()
                .ForSourceMember(src => src.Roles, opt => opt.DoNotValidate());
        }
    }
}