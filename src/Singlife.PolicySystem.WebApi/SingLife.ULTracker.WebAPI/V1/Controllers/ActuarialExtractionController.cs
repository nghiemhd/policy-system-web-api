using MediatR;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.LongRunningTasks.ActuarialExtraction;
using SingLife.ULTracker.WebAPI.Contracts.LongRunningTasks;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/actuarial-extraction")]
    public class ActuarialExtractionController : ControllerBase
    {
        private readonly IMediator mediator;

        public ActuarialExtractionController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [Route("icsif3a")]
        [CorrelatedAuditApi("ActuarialExtraction:IcsIF3A")]
        public async Task<IActionResult> ExportIcsIF3A([FromBody] ExportActuarialExtractionRequest request, CancellationToken cancellationToken)
        {
            var command = new GenerateIcsIF3ACommand
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }

        [HttpPost]
        [Route("icsmvmt4")]
        [CorrelatedAuditApi("ActuarialExtraction:IcsMvmt4")]
        public async Task<IActionResult> ExportIcsMvmt4([FromBody] ExportActuarialExtractionRequest request, CancellationToken cancellationToken)
        {
            var command = new GenerateIcsMvmt4Command
            {
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                UserName = request.UserName
            };

            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}