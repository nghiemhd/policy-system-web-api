using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.LongRunningTasks;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.LongRunningTasks;
using System.Threading;
using System.Threading.Tasks;
using FileContract = SingLife.ULTracker.WebAPI.Contracts.Common.File;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/mass-policy-statement-tasks")]
    public class MassPolicyStatementTaskController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public MassPolicyStatementTaskController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpPost]
        [CorrelatedAuditApi("MassPolicyStatementTask:Create")]
        public async Task<IActionResult> CreateTask([FromBody] GenerateMassPolicyStatementsRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<GenerateMassPolicyStatementsCommand>(request);

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<MassPolicyStatementTask>))]
        public async Task<IActionResult> GetTasks([FromQuery] SearchTasksRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchMassPolicyStatementTasksQuery>(request);

            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<MassPolicyStatementTask>>(result));
        }

        [HttpGet]
        [Route("{taskId}/zip-file")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContract))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetZipFile(long taskId, CancellationToken cancellationToken)
        {
            var query = new GetMassPolicyStatementTaskZipFileQuery() { TaskId = taskId };

            var document = await mediator.Send(query, cancellationToken);

            if (document == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<FileContract>(document));
        }

        [HttpDelete]
        [Route("{taskId:long}")]
        [CorrelatedAuditApi("MassPolicyStatementTask:Delete")]
        public async Task<IActionResult> DeleteTask(long taskId, CancellationToken cancellationToken)
        {
            var command = new DeleteMassPolicyStatementTaskCommand
            {
                TaskId = taskId
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}