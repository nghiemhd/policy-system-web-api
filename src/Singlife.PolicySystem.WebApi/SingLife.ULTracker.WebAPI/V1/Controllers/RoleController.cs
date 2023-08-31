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
    [Route("api/v{api-version:apiVersion}/roles")]
    public class RoleController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public RoleController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Role[]))]
        public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
        {
            var query = new GetAllRolesQuery();

            var roleDtos = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<Role[]>(roleDtos));
        }

        [HttpGet]
        [Route("{roleId:guid}")]
        public async Task<IActionResult> GetRole(Guid roleId, CancellationToken cancellationToken)
        {
            var query = new GetRoleDetailsQuery { Id = roleId };
            var roleDto = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<Role>(roleDto));
        }

        [HttpPost]
        [CorrelatedAuditApi("Role:Create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole(CreateRoleRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = mapper.Map<CreateRoleCommand>(request);
                var roleId = await mediator.Send(command, cancellationToken);

                return Ok(roleId);
            }
            catch (DuplicateRoleNameException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut]
        [CorrelatedAuditApi("Role:Update")]
        public async Task<IActionResult> UpdateRole(UpdateRoleRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<UpdateRoleCommand>(request);
            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpDelete]
        [Route("{id}")]
        [CorrelatedAuditApi("Role:Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new DeleteRoleCommand { Id = id };
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (RoleCannotBeDeletedException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}