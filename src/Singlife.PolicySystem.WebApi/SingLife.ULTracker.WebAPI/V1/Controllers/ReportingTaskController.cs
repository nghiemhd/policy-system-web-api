using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.LongRunningTasks;
using SingLife.ULTracker.UseCases.LongRunningTasks.ReportingTasks;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.LongRunningTasks;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/reporting-tasks")]
    public class ReportingTaskController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public ReportingTaskController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedSearchResult<ReportingTask>))]
        public async Task<IActionResult> GetReportingTasks([FromQuery] SearchReportingTasksRequest request, CancellationToken cancellationToken)
        {
            var query = new SearchReportingTasksQuery
            {
                PageIndex = request.CurrentPage - 1,
                PageSize = request.PageSize,
                ReportType = request.ReportType
            };

            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<ReportingTask>>(result));
        }

        [HttpGet]
        [Route("report-file")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(File))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetReportingTaskFile(string reportFileKey, CancellationToken cancellationToken)
        {
            var query = new GetReportingTaskFileQuery
            {
                ReportFileKey = reportFileKey
            };

            var reportFile = await mediator.Send(query, cancellationToken);

            if (reportFile == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<File>(reportFile));
        }
    }
}