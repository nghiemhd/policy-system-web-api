using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SingLife.ULTracker.UseCases.Common.Policies;
using SingLife.ULTracker.WebAPI.Contracts.Search;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.ULTracker.WebAPI.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/searches")]
    public class SearchController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public SearchController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpPost]
        [Route("policies")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchPoliciesResponse))]
        public async Task<IActionResult> SearchPolicies(SearchPoliciesRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<SearchPoliciesQuery>(request);
            var result = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<SearchPoliciesResponse>(result));
        }
    }
}