using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.UseCases.Customers;
using SingLife.ULTracker.UseCases.Ulpb.V1.CustomerNotification;
using SingLife.ULTracker.UseCases.Ulpb.V1.Policies;
using SingLife.ULTracker.UseCases.Underwriting;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Underwritings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/uw-requirements")]
    public class UnderwritingRequirementController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UnderwritingRequirementController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        /// <summary>
        /// Gets the underwriting requirements that are associated with the specified policy.
        /// </summary>
        /// <param name="policyId">The policy ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The underwriting requirements associated with the policy.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnderwritingRequirementWithRequest[]))]
        public async Task<IActionResult> GetRequirementsByPolicy(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetUnderwritingRequirementsQuery { PolicyId = policyId };
            var requirements = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<UnderwritingRequirementWithRequest[]>(requirements));
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnderwritingRequirementWithRequest))]
        public async Task<IActionResult> GetRequirementById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetRequirementQuery { RequirementId = id };
            var requirement = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<UnderwritingRequirementWithRequest>(requirement));
        }

        /// <summary>
        /// Searches for pending underwriting requirements by the specified search term.
        /// </summary>
        /// <param name="request"> request from uri and it contains:
        /// Search term.
        /// Page index, default to 0.
        /// Page size, default to 10.
        /// Accessible products.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Matched underwriting requirements, and the total number of matched records.</returns>
        [HttpGet]
        [Route("pending-requirements")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<MatchedUnderwritingRequirement>))]
        public async Task<IActionResult> SearchPendingUnderwritingRequirements(
            [FromQuery] SearchUnderwritingRequest request,
            CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchPendingUnderwritingRequirementsQuery>(request);
            var pagedSearchResult = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<MatchedUnderwritingRequirement>>(pagedSearchResult));
        }

        /// <summary>
        /// Creates a new underwriting requirement.
        /// </summary>
        /// <param name="request">A contract object representing the details of
        /// the new underwriting requirement to be created.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>Created (201)</term>
        /// <description>The underwriting requirement was created successfully.</description>
        /// </item>
        /// <item>
        /// <term>Bad request (400)</term>
        /// <description>The request was invalid, or the associated policy was not found, or the associated
        /// life assured snapshot was not found.</description>
        /// </item>
        /// </list>
        /// </returns>
        [HttpPost]
        [CorrelatedAuditApi("UWRequirement:Create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRequirement([FromBody] CreateUnderwritingRequirementRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return await CreateRequirementAsync();
            }
            catch (PolicyNotFoundException policyNotFoundException)
            {
                return NotFound(policyNotFoundException.Message);
            }
            catch (LifeAssuredSnapshotNotFoundException lifeAssuredSnapshotNotFoundException)
            {
                return NotFound(lifeAssuredSnapshotNotFoundException.Message);
            }

            async Task<IActionResult> CreateRequirementAsync()
            {
                var command = mapper.Map<CreateUnderwritingRequirementCommand>(request);
                var requirementId = await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        /// <summary>
        /// Updates the underwriting requirement specified by the <paramref name="requirementId"/>.
        /// </summary>
        /// <param name="requirementId">The ID of the underwriting requirement that will be updated.</param>
        /// <param name="request">A contract object representing the details of the updated underwriting requirement.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>OK (200)</term>
        /// <description>The underwriting requirement was updated successfully.</description>
        /// </item>
        /// <item>
        /// <term>Bad request (400)</term>
        /// <description>The request was invalid.</description>
        /// </item>
        /// <item>
        /// <term>Not found (404)</term>
        /// <description>The underwriting requirement was not found.</description>
        /// </item>
        /// </list>
        /// </returns>
        [HttpPut]
        [Route("{requirementId:guid}")]
        [CorrelatedAuditApi("UWRequirement:Update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRequirement(
            Guid requirementId,
            [FromBody] UpdateUnderwritingRequirementRequest request,
            CancellationToken cancellationToken)
        {
            CheckRequirementId(requirementId, request.RequirementId);

            try
            {
                return await UpdateRequirementAsync();
            }
            catch (UnderwritingRequirementNotFoundException)
            {
                return NotFound();
            }

            async Task<IActionResult> UpdateRequirementAsync()
            {
                request.RequirementId = requirementId;
                var command = mapper.Map<UpdateUnderwritingRequirementCommand>(request);

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        /// <summary>
        /// Deletes the specified underwriting requirement.
        /// </summary>
        /// <param name="requirementId">The ID of the underwriting requirement to be deleted.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An HTTP status code of No Content (204).</returns>
        [HttpDelete]
        [Route("{requirementId:guid}")]
        [CorrelatedAuditApi("UWRequirement:Delete")]
        public async Task<IActionResult> DeleteRequirement(Guid requirementId, CancellationToken cancellationToken)
        {
            var command = new DeleteUnderwritingRequirementCommand { RequirementId = requirementId };
            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        /// <summary>
        /// Gets the underwriting attachments of the specified underwriting requirement.
        /// </summary>
        /// <param name="requirementId">The ID of the underwriting requirement associated to the attachments.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// The underwriting attachments associated to the specified underwriting requirement.
        /// If no requirement is found, empty array is returned.
        /// </returns>
        [HttpGet]
        [Route("{requirementId:guid}/attachments")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UnderwritingAttachmentSummary>))]
        public async Task<IActionResult> GetRequirementAttachmentsDetails(Guid requirementId, CancellationToken cancellationToken)
        {
            var query = new GetUnderwritingAttachmentsByRequirementQuery { RequirementId = requirementId };

            var attachmentsDetails = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<IEnumerable<UnderwritingAttachmentSummary>>(attachmentsDetails));
        }

        [HttpPost]
        [Route("send-emails")]
        [CorrelatedAuditApi("Underwriting:SendEmailToAdviserAndConcierge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SendEmailToConierge(Guid policyId, string[] additionalEmails, CancellationToken cancellationToken)
        {
            var query = new GetPolicyQuery { PolicyId = policyId };
            var policy = await mediator.Send(query);

            if (policy == null)
            {
                return NotFound();
            }

            var sendEmailToCustomerCommand = new SendEmailToCustomerCommand
            {
                Policy = policy,
                AdditionalRecpientEmails = additionalEmails,
                EmailType = EmailType.OutstandingRequirementsEmail
            };

            await mediator.Send(sendEmailToCustomerCommand, cancellationToken);

            return Ok();
        }

        private void CheckRequirementId(Guid requirementIdInUri, Guid requirementIdInRequestBody)
        {
            if (requirementIdInUri != requirementIdInRequestBody)
            {
                throw new BadHttpRequestException("Underwriting requirement IDs specified in URI and request body are not matched.");
            }
        }
    }
}