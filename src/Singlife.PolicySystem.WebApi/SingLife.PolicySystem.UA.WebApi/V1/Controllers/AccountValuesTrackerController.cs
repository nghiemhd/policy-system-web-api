using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SingLife.PolicySystem.Shared.Audit.WebApi;
using SingLife.PolicySystem.UA.UseCases.AccountValues;
using SingLife.ULTracker.WebAPI.Contracts.Common;
using SingLife.ULTracker.WebAPI.Contracts.Ua.AccountValues;
using System.Threading;
using System.Threading.Tasks;

namespace SingLife.PolicySystem.UA.WebApi.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{api-version:apiVersion}/sa-account-values-tracker")]
    public class AccountValuesTrackerController : ControllerBase
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public AccountValuesTrackerController(IMediator mediator, IMapper mapper)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        [HttpGet]
        [Route("records")]
        public async Task<IActionResult> GetPagedRecords([FromQuery] GetAccountValuesTrackerRecordsRequest request, CancellationToken cancellationToken)
        {
            var query = mapper.Map<GetPagedAccountValuesTrackerRecordsQuery>(request);

            var pagedAVTrackerRecordDtos = await mediator.Send(query, cancellationToken);

            return Ok(mapper.Map<PagedSearchResult<AccountValuesTrackerRecord>>(pagedAVTrackerRecordDtos));
        }

        [HttpPost]
        [Route("recalculate")]
        [CorrelatedAuditApi("SAAccountValuesTracker:RecalculateAccountValues")]
        public async Task<IActionResult> RecalculateAccountValues([FromBody] RecalculateAccountValuesForTrackedPoliciesRequest request, CancellationToken cancellationToken)
        {
            var command = mapper.Map<RecalculateLeftoverAccountValuesOfTerminatedPoliciesCommand>(request);

            await mediator.Send(command, cancellationToken);

            return Ok();
        }
    }
}