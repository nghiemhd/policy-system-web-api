using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.ULTracker.UseCases.SystemTime;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/system-time")]
    public class SystemTimeController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public SystemTimeController(
            IMediator mediator,
            IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClockOffset))]
        public async Task<IActionResult> GetClockOffset(CancellationToken cancellationToken)
        {
            var offset = await mediator.Send(new GetClockOffsetQuery(), cancellationToken);

            return Ok(mapper.Map<ClockOffset>(offset));
        }

        [HttpPost]
        [CorrelatedAuditApi("ClockOffset:Update")]
        public async Task<IActionResult> UpdateClockOffset(ClockOffset request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<UpdateClockOffsetCommand>(request);

            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}