using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.Model.Common.Policies;
using SingLife.ULTracker.Model.Common.Policies.Documents;
using SingLife.ULTracker.UseCases.Underwriting;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.UnderwritingAttachments;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/underwriting-attachments")]
    public class UnderwritingAttachmentController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UnderwritingAttachmentController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(File))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetUnderwritingAttachmentQuery { AttachmentId = id };
            var attachment = await mediator.Send(query, cancellationToken);

            if (attachment == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<File>(attachment));
        }

        [HttpPost]
        [CorrelatedAuditApi("UWAttachment:Create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateUnderwritingAttachmentsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.RequirementId.HasValue)
                {
                    await CreateUnderwritingAttachmentsForRequirementAsync(request.RequirementId.Value, request.Attachments, cancellationToken);
                }
                else
                {
                    await CreateUnderwritingAttachmentsForPolicyAsync(request.PolicyId, request.Attachments, cancellationToken);
                }

                return Ok();
            }
            catch (PolicyNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (UnderwritingRequirementNotFoundException exception)
            {
                return NotFound(exception.Message);
            }
            catch (DuplicateAttachmentException exception)
            {
                return BadRequest(exception.Message);
            }
        }

        private async Task CreateUnderwritingAttachmentsForRequirementAsync(Guid requirementId, UnderwritingAttachment[] attachments, CancellationToken cancellationToken)
        {
            var command = new CreateUnderwritingAttachmentsForRequirementCommand
            {
                RequirementId = requirementId,
                Attachments = mapper.Map<UnderwritingAttachmentDto[]>(attachments)
            };

            await mediator.Send(command, cancellationToken);
        }

        private async Task CreateUnderwritingAttachmentsForPolicyAsync(Guid policyId, UnderwritingAttachment[] attachments, CancellationToken cancellationToken)
        {
            var command = new CreateUnderwritingAttachmentsForPolicyCommand
            {
                PolicyId = policyId,
                Attachments = mapper.Map<UnderwritingAttachmentDto[]>(attachments)
            };

            await mediator.Send(command, cancellationToken);
        }

        [HttpDelete]
        [Route("{id:guid}")]
        [CorrelatedAuditApi("UWAttachment:Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var command = new DeleteUnderwritingAttachmentCommand { AttachmentId = id };
            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}