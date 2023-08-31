using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.UA.UseCases.Checklists;
using SingLife.ULTracker.Model.Checklists;
using SingLife.ULTracker.WebAPI.Contracts.UA.Checklist;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.UA.WebApi.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ua-checklists")]
    public class UAChecklistController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UAChecklistController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("items-for-update/{checklistItemId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChecklistItemForUpdate))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChecklistItemForUpdate(Guid checklistItemId, CancellationToken cancellationToken)
        {
            var query = new GetChecklistItemForUpdateQuery { ChecklistItemId = checklistItemId };

            var checklistItem = await mediator.Send(query, cancellationToken);

            if (checklistItem == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<ChecklistItemForUpdate>(checklistItem));
        }

        [HttpPatch]
        [Route("items/{id:guid}")]
        [CorrelatedAuditApi("Checklist:UpdateChecklistItem")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChecklistItem(Guid id, [FromBody] UpdateChecklistItemRequest request, CancellationToken cancellationToken)
        {
            if (InvalidRequest())
            {
                return BadRequest();
            }

            try
            {
                return await UpdateChecklistItemAsync();
            }
            catch (ChecklistNotFoundException checklistItemNotFoundException)
            {
                return BadRequest(checklistItemNotFoundException.Message);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(exception.Message);
            }

            bool InvalidRequest()
            {
                return id != request.ChecklistItemId;
            }

            async Task<IActionResult> UpdateChecklistItemAsync()
            {
                var command = mapper.Map<ManuallyChangeChecklistItemCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }

        [HttpPatch]
        [Route("items/{id:guid}/comment")]
        [CorrelatedAuditApi("Checklist:UpdateChecklistItemComment")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateChecklistItemComment(Guid id, [FromBody] UpdateChecklistItemCommentRequest request, CancellationToken cancellationToken)
        {
            if (InvalidRequest())
            {
                return BadRequest();
            }

            try
            {
                var command = mapper.Map<UpdateChecklistItemCommentCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
            catch (ChecklistNotFoundException checklistItemNotFoundException)
            {
                return BadRequest(checklistItemNotFoundException.Message);
            }

            bool InvalidRequest()
            {
                return id != request.ChecklistItemId;
            }
        }
    }
}