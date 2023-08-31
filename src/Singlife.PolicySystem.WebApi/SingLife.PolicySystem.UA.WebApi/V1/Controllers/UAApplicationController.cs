using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.UA.UseCases.Applications;
using SingLife.ULTracker.WebAPI.Contracts.UA.Applications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.UA.WebApi.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ua-applications")]
    public class UAApplicationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UAApplicationController(
          IMediator mediator,
          IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{applicationId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UAApplicationSummary))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid applicationId, CancellationToken cancellationToken)
        {
            var query = new GetApplicationSummaryQuery
            {
                ApplicationId = applicationId
            };

            var applicationSummaryDto = await mediator.Send(query, cancellationToken);

            if (applicationSummaryDto == null)
            {
                return NotFound();
            }

            return Ok(mapper.Map<UAApplicationSummary>(applicationSummaryDto));
        }
    }
}