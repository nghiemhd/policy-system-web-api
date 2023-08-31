using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Underwritings;
using SingLife.ULTracker.UseCases.Underwriting;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Underwritings;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnderwritingRequest = SingLife.ULTracker.WebAPI.Contracts.Underwritings.UnderwritingRequest;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/uw-requests")]
    public class UnderwritingRequestController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UnderwritingRequestController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        /// <summary>
        /// Searches for underwriting requests by the specified search term.
        /// </summary>
        /// <param name="request"> request from uri and it contains:
        /// Search term.
        /// Page index, default to 0.
        /// Page size, default to 10.
        /// Accessible products.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Matched underwriting requests, and the total number of matched records.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<MatchedUnderwritingRequest>))]
        public async Task<IActionResult> SearchUnderwritingRequests(
            [FromQuery] SearchUnderwritingRequest request,
            CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchUnderwritingRequestsQuery>(request);
            var pagedSearchResult = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<MatchedUnderwritingRequest>>(pagedSearchResult));
        }

        /// <summary>
        /// Searches for pending underwriting requests by the specified search term.
        /// </summary>
        /// <param name="request"> request from uri and it contains:
        /// Search term.
        /// Page index, default to 0.
        /// Page size, default to 10.
        /// Accessible products.
        /// </param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Matched underwriting requests, and the total number of matched records.</returns>
        [HttpGet]
        [Route("pending-requests")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<MatchedUnderwritingRequest>))]
        public async Task<IActionResult> SearchPendingUnderwritingRequests(
            [FromQuery] SearchUnderwritingRequest request,
            CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchPendingUnderwritingRequestsQuery>(request);
            var pagedSearchResult = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<MatchedUnderwritingRequest>>(pagedSearchResult));
        }

        /// <summary>
        /// Creates a new underwriting request.
        /// </summary>
        /// <param name="requirementId">The ID of the underwriting requirement associated to the request.</param>
        /// <param name="request">A contract object representing the details of the new underwriting request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>Created (201)</term>
        /// <description>The underwriting request was created successfully.</description>
        /// </item>
        /// <item>
        /// <term>Bad request (400)</term>
        /// <description>The request was invalid, or the associated underwriting requirement was not found.</description>
        /// </item>
        /// </list>
        /// </returns>
        [HttpPost]
        [Route("{requirementId:guid}")]
        [CorrelatedAuditApi("UWRequest:Create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateRequest(
            Guid requirementId,
            [FromBody] CreateUnderwritingRequestRequest request,
            CancellationToken cancellationToken)
        {
            CheckRequirementId(requirementId, request.RequirementId);

            try
            {
                return await CreateRequestAsync();
            }
            catch (UnderwritingRequirementNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (RequestPeriodIsOverlappedException exception)
            {
                return BadRequest(exception.Message);
            }

            async Task<IActionResult> CreateRequestAsync()
            {
                request.RequirementId = requirementId;
                var command = mapper.Map<CreateUnderwritingRequestCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpGet]
        [Route("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UnderwritingRequest))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRequestById(Guid id, CancellationToken cancellationToken)
        {
            var matchedRequestDto = await QueryUnderwritingRequestAsync();

            if (matchedRequestDto == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<UnderwritingRequest>(matchedRequestDto));

            Task<UnderwritingRequestDto> QueryUnderwritingRequestAsync()
            {
                var query = new GetUnderwritingRequestQuery { Id = id };
                return mediator.Send(query, cancellationToken);
            }
        }

        /// <summary>
        /// Updates the underwriting request specified by the <paramref name="requestId"/>.
        /// </summary>
        /// <param name="requestId">The ID of the underwriting request that will be updated.</param>
        /// <param name="request">A contract object representing the details of the updated underwriting request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// <term>OK (200)</term>
        /// <description>The underwriting request was updated successfully.</description>
        /// </item>
        /// <item>
        /// <term>Bad request (400)</term>
        /// <description>The request was invalid, or the associated underwriting requirement was not found.</description>
        /// </item>
        /// <item>
        /// <term>Not found (404)</term>
        /// <description>The underwriting request was not found.</description>
        /// </item>
        /// </list>
        /// </returns>
        [HttpPut]
        [Route("{requestId:guid}")]
        [CorrelatedAuditApi("UWRequest:Update")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRequest(
            Guid requestId,
            [FromBody] UpdateUnderwritingRequestRequest request,
            CancellationToken cancellationToken)
        {
            CheckRequestId(requestId, request.RequestId);

            try
            {
                return await UpdateRequestAsync();
            }
            catch (UnderwritingRequirementNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (UnderwritingRequestNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (RequestPeriodIsOverlappedException exception)
            {
                return BadRequest(exception.Message);
            }

            async Task<IActionResult> UpdateRequestAsync()
            {
                request.RequestId = requestId;
                var command = mapper.Map<UpdateUnderwritingRequestCommand>(request);

                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        /// <summary>
        /// Deletes the specified underwriting request.
        /// </summary>
        /// <param name="requestId">The ID of the underwriting request to be deleted.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An HTTP status code of No Content (204).</returns>
        [HttpDelete]
        [Route("{requestId:guid}")]
        [CorrelatedAuditApi("UWRequest:Delete")]
        public async Task<IActionResult> DeleteRequest(Guid requestId, CancellationToken cancellationToken)
        {
            var command = new DeleteUnderwritingRequestCommand { RequestId = requestId };
            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        private void CheckRequirementId(Guid requirementIdInUri, Guid requirementIdInRequestBody)
        {
            if (requirementIdInUri != requirementIdInRequestBody)
            {
                throw new BadHttpRequestException("Underwriting requirement IDs specified in URI and request body are not matched.");
            }
        }

        private void CheckRequestId(Guid requestIdInUri, Guid requestIdInRequestBody)
        {
            if (requestIdInUri != requestIdInRequestBody)
            {
                throw new BadHttpRequestException("Underwriting request IDs specified in URI and request body are not matched.");
            }
        }
    }
}