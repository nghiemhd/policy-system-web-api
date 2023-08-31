using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.Auth;
using SingLife.ULTracker.WebAPI.Contracts.Roles;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/permission-groups")]
    public class PermissionGroupController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public PermissionGroupController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionGroup[]))]
        public async Task<IActionResult> GetAllPermissionGroups(CancellationToken cancellationToken)
        {
            var query = new GetAllPermissionGroupsQuery();
            var permissionGroupDtos = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PermissionGroup[]>(permissionGroupDtos));
        }

        [HttpGet]
        [Route("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionGroup))]
        public async Task<IActionResult> GetPermissionGroup(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetPermissionGroupQuery { PermissionGroupId = id };
            var permissionGroupDto = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PermissionGroup>(permissionGroupDto));
        }

        [HttpPut]
        [CorrelatedAuditApi("PermissionGroup:Update")]
        public async Task<IActionResult> UpdatePermissionGroup(UpdatePermissionGroupRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<UpdatePermissionGroupCommand>(request);
            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}