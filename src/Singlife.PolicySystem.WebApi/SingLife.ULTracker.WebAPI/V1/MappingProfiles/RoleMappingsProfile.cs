using AutoMapper;
using SingLife.ULTracker.UseCases.Auth;
using SingLife.ULTracker.UseCases.Auth.DTOs;
using SingLife.ULTracker.WebAPI.Contracts.Roles;

namespace SingLife.ULTracker.WebAPI.V1.MappingProfiles
{
    public class RoleMappingsProfile : Profile
    {
        public RoleMappingsProfile()
        {
            CreateMap<PermissionGroupDto, PermissionGroup>()
                .ReverseMap();

            CreateMap<PermissionDto, Permission>()
                .ReverseMap();

            CreateMap<RoleDto, Role>();

            CreateMap<CreateRoleRequest, CreateRoleCommand>();

            CreateMap<UpdateRoleRequest, UpdateRoleCommand>();

            CreateMap<UpdatePermissionRequest, UpdatePermissionCommand>();

            CreateMap<UpdatePermissionGroupRequest, UpdatePermissionGroupCommand>();
        }
    }
}