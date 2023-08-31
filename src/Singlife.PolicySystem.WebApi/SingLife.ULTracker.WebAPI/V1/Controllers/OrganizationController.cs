using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.Common.Customers;
using SingLife.ULTracker.UseCases.Customers;
using SingLife.ULTracker.WebAPI.Contracts.Customers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/organizations")]
    public class OrganizationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public OrganizationController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrganisationDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrganizationDetails(Guid id, [FromQuery] string[] products, CancellationToken cancellationToken)
        {
            var query = new GetOrganisationDetailsQuery() { OrganisationId = id, Products = products };
            var organization = await mediator.Send(query, cancellationToken);

            if (organization != null)
            {
                return Ok(mapper.Map<OrganisationDetails>(organization));
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        [Route("details/{bizRegNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrganisationDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrganizationDetailsByBizRegNumber(string bizRegNumber, CancellationToken cancellationToken)
        {
            var query = new GetOrganisationByBizRegNumberQuery { BizRegNumber = bizRegNumber };

            var result = await mediator.Send(query, cancellationToken);

            if (result == null) return NotFound();

            return Ok(mapper.Map<OrganisationDetails>(result));
        }

        [HttpGet]
        [Route("search")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OrganisationDetails[]))]
        public async Task<IActionResult> SearchOrganisationByBizRegNumber([FromQuery] string bizRegNumber)
        {
            var query = new SearchOrganisationsByBizRegNumberQuery { BizRegNumber = bizRegNumber };

            var organisationDtos = await mediator.Send(query);

            return Ok(mapper.Map<OrganisationDetails[]>(organisationDtos));
        }

        [HttpPut]
        [CorrelatedAuditApi("Organization:Update")]
        public async Task<IActionResult> UpdateOrganization([FromBody] UpdateOrganisationRequest request)
        {
            var command = new UpdateOrganisationCommand
            {
                OrganizationDetails = mapper.Map<OrganisationDetailsDto>(request.OrganizationDetails),
                UniqueCorrelationId = this.GetUniqueCorrelationId(),
                AuthenticatedUser = this.GetAuthenticatedUser()
            };

            await mediator.Send(command);

            return Ok();
        }

        [HttpPost]
        [Route("others")]
        [CorrelatedAuditApi("OtherOrganisation:Create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOtherOrganisation(CreateOtherOrganisationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var organisation = await mediator.Send(new GetOrganisationByBizRegNumberQuery { BizRegNumber = request.BizRegNumber }, cancellationToken);

                Guid organisationId;

                if (organisation != null)
                {
                    organisationId = organisation.OrganizationId;

                    await mediator.Send(new UpdateAuthorisedPersonsCommand
                    {
                        OrganisationId = organisation.OrganizationId,
                        AuthorisedPersons = mapper.Map<AuthorisedPersonDto[]>(request.AuthorisedPersons)
                    });
                }
                else
                {
                    var organisationIdentity = await mediator.Send(mapper.Map<CreateOrganisationCommand>(request), cancellationToken);

                    organisationId = organisationIdentity.OrganisationId;
                }

                var command = mapper.Map<CreateOtherOrganisationCommand>(request);
                command.OrganisationId = organisationId;

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (OrganisationAlreadyExistsException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("others/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Organisation))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOtherOrganisation(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                return await QueryOrganisationSnapshotAsync();
            }
            catch (OrganisationSnapshotNotFoundException)
            {
                return NotFound();
            }

            async Task<IActionResult> QueryOrganisationSnapshotAsync()
            {
                var query = new GetOrganisationSnapshotQuery { Id = id };

                var organisationSnapshotDto = await mediator.Send(query, cancellationToken);

                return Ok(mapper.Map<Organisation>(organisationSnapshotDto));
            }
        }

        [HttpPatch]
        [Route("others")]
        [CorrelatedAuditApi("OtherOrganisation:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditOtherOrganisation(EditOtherOrganisationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.EditAllFields)
                {
                    return await EditOtherOrganisationAsync();
                }

                return await EditOtherOrganisationPartiallyAsync();
            }
            catch (OrganisationSnapshotNotFoundException)
            {
                return NotFound();
            }

            async Task<IActionResult> EditOtherOrganisationAsync()
            {
                var command = mapper.Map<EditOtherOrganisationCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }

            async Task<IActionResult> EditOtherOrganisationPartiallyAsync()
            {
                var command = mapper.Map<EditOtherOrganisationPartiallyCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpDelete]
        [Route("others")]
        [CorrelatedAuditApi("OtherOrganization:Delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOtherOrganisation([FromQuery] DeleteOtherOrganisationRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await DeleteOtherOrganisationAsync();
            }
            catch (OrganisationSnapshotNotFoundException)
            {
                return NotFound();
            }

            async Task<IActionResult> DeleteOtherOrganisationAsync()
            {
                var command = mapper.Map<DeleteOrganisationSnapshotCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }
    }
}