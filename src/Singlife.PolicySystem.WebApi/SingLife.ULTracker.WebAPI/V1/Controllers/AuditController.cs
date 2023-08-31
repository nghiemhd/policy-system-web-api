using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.Audit;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.WebAPI.Contracts.Audits;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using System;
using System.Threading;
using System.Threading.Tasks;
using ValueChangesGroupContract = SingLife.ULTracker.WebAPI.Contracts.Audits.ValueChangesGroup;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/audit-events")]
    public class AuditController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public AuditController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("value-changes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ValueChangesGroupContract[]))]
        public async Task<IActionResult> GetValueChanges([FromQuery] GetValueChangesRequest request, CancellationToken cancellationToken)
        {
            var valueChangesQuery = mapper.Map<GetValueChangesQuery>(request);

            var valueChanges = await mediator.Send(valueChangesQuery, cancellationToken);

            return Ok(mapper.Map<ValueChangesGroupContract[]>(valueChanges));
        }

        [HttpGet]
        [Route("policy/{policyId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuditEvent[]))]
        public async Task<IActionResult> GetPolicyAuditEvents(Guid policyId, CancellationToken cancellationToken)
        {
            var policyAndApplicationIdentitiesQuery = new GetPolicyAndApplicationIdentitiesQuery
            {
                PolicyId = policyId
            };

            var policyAndApplicationIdentities = await mediator.Send(policyAndApplicationIdentitiesQuery, cancellationToken);

            var policyAuditEventsQuery = new GetPolicyAuditEventsQuery
            {
                PolicyId = policyId,
                ExternalPolicyId = policyAndApplicationIdentities.ExternalPolicyId,
            };

            var auditEventDtos = await mediator.Send(policyAuditEventsQuery, cancellationToken);

            return Ok(mapper.Map<AuditEvent[]>(auditEventDtos));
        }

        [HttpGet]
        [Route("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuditEvent[]))]
        public async Task<IActionResult> GetCustomerAuditEvents(Guid customerId, CancellationToken cancellationToken)
        {
            var customerAuditEventsQuery = new GetCustomerAuditEventQuery
            {
                CustomerId = customerId
            };

            var auditEventDtos = await mediator.Send(customerAuditEventsQuery, cancellationToken);

            return Ok(mapper.Map<AuditEvent[]>(auditEventDtos));
        }

        [HttpGet]
        [Route("organisation/{organisationId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuditEvent[]))]
        public async Task<IActionResult> GetOrganisationAuditEvents(Guid organisationId, CancellationToken cancellationToken)
        {
            var organisationAuditEventsQuery = new GetOrganisationAuditEventQuery
            {
                OrganisationId = organisationId
            };

            var auditEventDtos = await mediator.Send(organisationAuditEventsQuery, cancellationToken);

            return Ok(mapper.Map<AuditEvent[]>(auditEventDtos));
        }

        [HttpGet]
        [Route("failed-audit-event/{webApiAuditEventId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FailedAuditEvent))]
        public async Task<IActionResult> GetFailedAuditEvent(long webApiAuditEventId, CancellationToken cancellationToken)
        {
            var query = new GetFailedAuditEventQuery { WebApiAuditEventId = webApiAuditEventId };

            var failedAuditEventDto = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<FailedAuditEvent>(failedAuditEventDto));
        }
    }
}