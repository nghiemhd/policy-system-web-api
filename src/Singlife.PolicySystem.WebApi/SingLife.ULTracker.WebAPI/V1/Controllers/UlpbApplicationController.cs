using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.Ulpb.V1.Applications;
using SingLife.ULTracker.WebAPI.Contracts.Ulpb.V1.Applications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/ulpb-applications")]
    public class UlpbApplicationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public UlpbApplicationController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("{id:guid}/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UlpbApplicationDetails))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetApplicationDetails(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetApplicationQuery { ApplicationId = id };
            var applicationDto = await mediator.Send(query, cancellationToken);

            if (applicationDto == null)
                return NotFound();

            return Ok(mapper.Map<UlpbApplicationDetails>(applicationDto));
        }
    }
}