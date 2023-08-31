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
    [Route("api/v{api-version:apiVersion}/permissions")]
    public class PermissionController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public PermissionController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Permission))]
        public async Task<IActionResult> GetPermission(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetPermissionQuery { PermissionId = id };
            var permissionDto = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<Permission>(permissionDto));
        }

        [HttpPut]
        [CorrelatedAuditApi("Permission:Update")]
        public async Task<IActionResult> UpdatePermission(UpdatePermissionRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<UpdatePermissionCommand>(request);
            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}