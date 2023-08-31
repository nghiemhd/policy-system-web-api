using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.Common;
using SingLife.ULTracker.UseCases.PolicyAttachments;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.PolicyAttachments;
using System;
using System.Threading;
using System.Threading.Tasks;
using DeletePolicyAttachmentCommand = SingLife.ULTracker.UseCases.PolicyAttachments.DeletePolicyAttachmentCommand;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/policy-attachments")]
    public class PolicyAttachmentController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public PolicyAttachmentController(IMapper mapper, IMediator mediator)
        {
            this.mapper = mapper;
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyRelatedAttachments))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPolicyRelatedAttachments([FromQuery] Guid? policyId, CancellationToken cancellationToken)
        {
            if (policyId == null)
                return BadRequest("Policy Id is required.");

            var query = new GetPolicyRelatedAttachmentsQuery { PolicyId = policyId.Value };
            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PolicyRelatedAttachments>(result));
        }

        [HttpPost]
        [CorrelatedAuditApi("Policy:AddAttachment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreatePolicyAttachments(CreatePolicyAttachmentsRequest request, CancellationToken cancellationToken)
        {
            var policyIdentities = await GetPolicyIdentitiesAsync(request.PolicyId, cancellationToken);

            if (policyIdentities == null)
                return NotFound();

            await CreatePolicyAttachmentsAsync(policyIdentities.PolicyNumber, request.Attachments, cancellationToken);

            return Ok();
        }

        private Task<PolicyApplicationIdentitiesDto> GetPolicyIdentitiesAsync(Guid policyId, CancellationToken cancellationToken)
        {
            var query = new GetPolicyAndApplicationIdentitiesQuery { PolicyId = policyId };
            return mediator.Send(query, cancellationToken);
        }

        private async Task CreatePolicyAttachmentsAsync(string policyNumber, PolicyAttachment[] policyAttachments, CancellationToken cancellationToken)
        {
            var command = new CreatePolicyAttachmentsCommand
            {
                PolicyNumber = policyNumber,
                PolicyAttachments = mapper.Map<PolicyAttachmentDto[]>(policyAttachments)
            };

            await mediator.Send(command, cancellationToken);
        }

        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(File))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadPolicyAttachment(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetPolicyAttachmentQuery() { PolicyAttachmentId = id };

            var fileDto = await mediator.Send(query, cancellationToken);

            if (fileDto == null)
                return NotFound();

            return Ok(mapper.Map<File>(fileDto));
        }

        [HttpDelete]
        [Route("{id}")]
        [CorrelatedAuditApi("Policy:DeleteAttachment")]
        public async Task<IActionResult> DeletePolicyAttachment(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeletePolicyAttachmentCommand() { PolicyAttachmentId = id };
            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}