using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.Common.Applications;
using SingLife.ULTracker.WebAPI.Contracts.Common.Applications;
using System;
using System.Threading;
using System.Threading.Tasks;
using SearchApplicationsResultContract = SingLife.ULTracker.WebAPI.Contracts.Common.Applications.SearchApplicationResult;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/applications")]
    public class ApplicationController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public ApplicationController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchApplicationsResultContract))]
        public async Task<IActionResult> Search(SearchApplicationRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchApplicationsQuery>(request);
            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<SearchApplicationsResultContract>(result));
        }

        [HttpGet]
        [Route("other-customers/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(OtherCustomer))]
        public async Task<IActionResult> GetOtherCustomer(Guid id, CancellationToken cancellationToken)
        {
            var otherCustomerDto = await QueryOtherCustomerAsync(id, cancellationToken);

            if (otherCustomerDto == null)
                return NotFound();

            return Ok(mapper.Map<OtherCustomer>(otherCustomerDto));
        }

        private async Task<OtherCustomerDto> QueryOtherCustomerAsync(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetOtherCustomerQuery { Id = id };

            return await mediator.Send(query, cancellationToken);
        }
    }
}