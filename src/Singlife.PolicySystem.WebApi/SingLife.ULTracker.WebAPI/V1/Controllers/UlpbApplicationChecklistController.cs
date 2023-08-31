using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.Ulpb.V1.Applications;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.ApplicationChecklists;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ulpb-application-checklists")]
    public class UlpbApplicationChecklistController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UlpbApplicationChecklistController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("items/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UlpbApplicationChecklistItem))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetChecklistItemById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetApplicationChecklistItemQuery
            {
                Id = id
            };

            var checklistItem = await mediator.Send(query, cancellationToken);
            if (checklistItem == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<UlpbApplicationChecklistItem>(checklistItem));
        }

        [HttpPatch]
        [Route("items/{id:guid}")]
        [CorrelatedAuditApi("ApplicationChecklist:Edit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangeChecklistItemStatus(Guid id, [FromBody] ChangeChecklistItemStatusRequest request, CancellationToken cancellationToken)
        {
            if (InvalidRequest())
            {
                return BadRequest();
            }

            try
            {
                return await ChangeChecklistItemStatusAsync();
            }
            catch (ApplicationChecklistItemNotFoundException checklistItemNotFoundException)
            {
                return BadRequest(checklistItemNotFoundException.Message);
            }

            bool InvalidRequest()
            {
                return id != request.Id;
            }

            async Task<IActionResult> ChangeChecklistItemStatusAsync()
            {
                var command = mapper.Map<ManuallyChangeChecklistItemStatusCommand>(request);
                await mediator.Send(command, cancellationToken);

                return Ok();
            }
        }
    }
}