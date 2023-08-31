using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Auth;
using SingLife.ULTracker.UseCases.Auth;
using SingLife.ULTracker.UseCases.Auth.DTOs;
using SingLife.ULTracker.WebAPI.Contracts.Roles;
using SingLife.ULTracker.WebAPI.Contracts.Users;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Role = SingLife.ULTracker.WebAPI.Contracts.Roles.Role;
using User = SingLife.ULTracker.WebAPI.Contracts.Users.User;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/users")]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UserController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User[]))]
        public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
        {
            var query = new GetUserListQuery();
            var queryUsersResult = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<User[]>(queryUsersResult));
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("permissions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Role[]))]
        public async Task<IActionResult> Permissions([FromQuery] string userName, CancellationToken cancellationToken)
        {
            RoleDto[] roleDtos;

            var cachedQuery = new GetCachedUserRolesAndPermissionsQuery { UserName = userName };
            roleDtos = await mediator.Send(cachedQuery, cancellationToken);

            if (roleDtos == null || !roleDtos.Any())
            {
                var query = new GetUserRolesAndPermissionsQuery { UserName = userName };
                roleDtos = await mediator.Send(query, cancellationToken);

                var command = new UpdateUserRolesAndPermissionsCacheCommand
                {
                    UserName = userName,
                    Roles = roleDtos
                };
                await mediator.Send(command, cancellationToken);
            }

            return Ok(mapper.Map<Role[]>(roleDtos));
        }

        [HttpDelete]
        [CorrelatedAuditApi("User:Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUsers([FromBody] DeleteUserRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<DeleteUserCommand>(request);

            try
            {
                await mediator.Send(command, cancellationToken);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("roles")]
        [CorrelatedAuditApi("User:AssignRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AssignRoles([FromBody] AssignRolesToUserRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<AssignRolesToUserCommand>(request);

            try
            {
                await mediator.Send(command, cancellationToken);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("editing-role")]
        [CorrelatedAuditApi("User:EditUserRoles")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditUserRoles([FromBody] EditUserRolesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<EditUserRolesCommand>(request);

            try
            {
                await mediator.Send(command, cancellationToken);
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpGet]
        [Route("search-by-role")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserQueriedByRole[]))]
        public async Task<IActionResult> GetUsersByRoleName(string roleName, CancellationToken cancellationToken)
        {
            var query = new GetUsersByRoleQuery() { RoleName = roleName };
            var queryUsersResult = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<UserQueriedByRole[]>(queryUsersResult));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUserByIdQuery() { Id = id };
            var user = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<User>(user));
        }

        [HttpPut]
        [Route("last-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUserLastLogin(UpdateUserLastLoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var command = new UpdateUserLastLoginCommand { UserEmail = request.UserEmail };
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}